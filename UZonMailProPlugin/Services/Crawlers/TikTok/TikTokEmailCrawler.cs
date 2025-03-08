using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Http;
using UZonMail.Utils.Json;
using UZonMailProPlugin.Services.Crawlers.ByteDance.APIs;
using UZonMailProPlugin.Services.Crawlers.ByteDance.Extensions;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.Crawlers.TiTok
{
    /// <summary>
    /// TikTok 邮箱爬虫
    /// </summary>
    public class TikTokEmailCrawler : CrawlerTaskBase
    {
        private SqlContext _db;
        private CrawlerManager _crawlerManager;
        private FunctionAccessService _access;

        public TikTokEmailCrawler(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            var provider = Scope.ServiceProvider;
            _db = provider.GetRequiredService<SqlContext>();
            _crawlerManager = provider.GetRequiredService<CrawlerManager>();
            _access = provider.GetRequiredService<FunctionAccessService>();
        }

        private static readonly ILog _logger = LogManager.GetLogger(typeof(TikTokEmailCrawler));
        private readonly int _delayMilliseconds = 1000;

        /// <summary>
        /// 执行具体的爬取任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CrawlerTaskParams crawlerTaskParams)
        {
            // 开始爬取
            var recommendGetter = new RecommendAuthorGetter(crawlerTaskParams);
            var recommendVideoInfo = await recommendGetter.Next();
            while (recommendVideoInfo != null && !RootStep.ShouldStop())
            {
                // 判断是否处于爬取中
                await StartRecommendStep(recommendVideoInfo, crawlerTaskParams);

                // 获取下一个
                recommendVideoInfo = await recommendGetter.Next();
            }
        }

        /// <summary>
        /// 单个推荐视频的爬取
        /// </summary>
        /// <param name="recommendVideoInfo"></param>
        /// <param name="crawlerTaskParams"></param>
        /// <returns></returns>
        private async Task StartRecommendStep(JObject recommendVideoInfo, CrawlerTaskParams crawlerTaskParams)
        {
            // 获取作者信息
            var authorInfo = recommendVideoInfo.SelectTokenOrDefault<TiktokAuthor>("author");
            if (authorInfo == null) return;

            var stepManager = crawlerTaskParams.StepManager;
            // 判断是否存在
            var existAuthor = await _db.TiktokAuthors
                .Where(x => x.Id == authorInfo.Id)
                .FirstOrDefaultAsync();

            // 已经存在
            if (existAuthor != null)
            {
                // 1.递归复制既有结果
                await CopyCrawledResultsRecursively(_db, authorInfo, crawlerTaskParams.CrawlerTaskId);
                // 判断是爬取的数量与总数是否相等，不相等，则继续爬取粉丝信息
                // [TODO] 未完成
                // 由于每个任务获取重复任务的概率太高了，因此若遇到，直接跳过
                return;
            }

            // 2. 等待保存新的结果
            if (stepManager.TryGetValue(authorInfo.Id, out var crawStep))
            {
                // 判断当前任务是否已经在爬取中
                if (crawStep.ExistsCrawlerTaskId(crawlerTaskParams.CrawlerTaskId)) return;

                // 添加到爬取任务中
                crawStep.AddParent(RootStep);
                await crawStep.ExecuteTask;
                crawStep.RemoveParent(RootStep);
            }
            else
            {
                // 任务未进行，添加新任务
                // 开启新任务
                var recommendStep = new RecommendStep(crawlerTaskParams, recommendVideoInfo, authorInfo.Id);
                recommendStep.AddParent(RootStep);
                stepManager.TryAdd(authorInfo.Id, recommendStep);
                await recommendStep.StartAsync();
                // 结束爬取, 移除缓存
                stepManager.TryRemove(authorInfo.Id, out _);
                recommendStep.RemoveParent(RootStep);
            }
        }

        public static async Task CopyCrawledResultsRecursively(SqlContext db, TiktokAuthor authorInfo, long crawlerTaskId)
        {
            if (authorInfo == null)
            {
                return;
            }

            // 判断是否存在
            var scrawlerResult = await db.CrawlerTaskResults
                .Where(x => x.CrawlerTaskInfoId == crawlerTaskId && x.TikTokAuthorId == authorInfo.Id)
                .FirstOrDefaultAsync();
            if (scrawlerResult != null)
            {
                // 说明已经存在了
                _logger.Debug($"已复制过 [{authorInfo.Nickname}] 粉丝数据，跳过操作");
                return;
            }

            // 不存在，添加
            var result = new CrawlerTaskResult()
            {
                TikTokAuthorId = authorInfo.Id,
                CrawlerTaskInfoId = crawlerTaskId,
                ExistExtraInfo = authorInfo.IsParsed
            };
            db.CrawlerTaskResults.Add(result);
            await db.SaveChangesAsync();

            // 获取粉丝信息
            var fans = await db.TiktokAuthors.AsNoTracking()
                .Where(x => x.FollowingAuthorId == authorInfo.Id)
                .Select(x => new TiktokAuthor() { Id = x.Id, IsParsed = x.IsParsed })
                .ToListAsync();

            foreach (var author in fans)
            {
                await CopyCrawledResultsRecursively(db, author, crawlerTaskId);
            }
        }
    }
}
