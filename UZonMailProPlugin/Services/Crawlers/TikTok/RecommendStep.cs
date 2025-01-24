using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Json;

namespace UZonMailProPlugin.Services.Crawlers.TiTok
{
    /// <summary>
    /// 获取推荐视频的用户
    /// </summary>
    /// <param name="crawlerTaskParams"></param>
    /// <param name="authorInfo"></param>
    /// <param name="recommendVideoInfo"></param>
    public class RecommendStep(CrawlerTaskParams crawlerTaskParams, JObject recommendVideoInfo, long authorId) : CrawlStep(authorId)
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(RecommendStep));
        private readonly SqlContext _db = crawlerTaskParams.ServiceProvider.GetRequiredService<SqlContext>();

        protected override async Task ExecuteAsync()
        {
            var authorInfo = recommendVideoInfo.SelectTokenOrDefault<TiktokAuthor>("author");
            if (authorInfo == null) return;

            // 保存作者信息
            await SaveAuthor(authorInfo);

            // 爬取粉丝
            await CrawlFolloers(authorInfo);

            // 移除缓存
            crawlerTaskParams.StepManager.TryRemove(authorInfo.Id, out _);
        }

        private async Task SaveAuthor(TiktokAuthor authorInfo)
        {
            // 判断是否存在，若存在，则不再保存
            var existOne = await _db.TiktokAuthors.FirstOrDefaultAsync(x => x.Id == authorInfo.Id);
            if (existOne != null) return;

            // 保存作者信息
            await _db.TiktokAuthors.AddAsync(authorInfo);
            await _db.SaveChangesAsync();

            // 记录广告分类 id
            var diversification = new TikTokAuthorDiversification()
            {
                TikTokAuthorId = authorInfo.Id,
                DiversificationId = recommendVideoInfo.SelectTokenOrDefault("diversificationId", 0),
            };
            if (diversification.DiversificationId > 0)
            {
                // 若不存在分类 id，则保存
                await _db.TikTokAuthorDiversifications.AddAsync(diversification);
            }

            // 记录统计信息
            var statsInfo = recommendVideoInfo.SelectTokenOrDefault<TikTokAuthStats>("statsV2");
            statsInfo?.SetTo(authorInfo);

            // 解析账号
            var resolver = new SignatureResolver(authorInfo.Signature);
            resolver?.ResolveFor(authorInfo);
            await _db.SaveChangesAsync();

            // 记录爬取结果
            await SaveCrawlerTaskResult(_db, authorInfo);
        }

        /// <summary>
        /// 为了性能，按深度优先的方式进行爬取
        /// </summary>
        /// <param name="authorInfo"></param>
        /// <returns></returns>
        protected async Task CrawlFolloers(TiktokAuthor authorInfo)
        {
            var followersGetter = new FollowersGetter(crawlerTaskParams, authorInfo.SecUid);
            var followerInfo = await followersGetter.Next();

            var stepManager = crawlerTaskParams.StepManager;

            while (followerInfo != null && !ShouldStop())
            {
                // 判断是否存在
                var existAuthor = await _db.TiktokAuthors
                    .Where(x => x.Id == authorInfo.Id)
                    .FirstOrDefaultAsync();

                // 已经存在
                if (existAuthor != null)
                {
                    // 1.递归复制既有结果
                    await TikTokEmailCrawler.CopyCrawledResultsRecursively(_db, authorInfo, crawlerTaskParams.CrawlerTaskId);
                }

                var followerId = followerInfo.SelectTokenOrDefault("user.id", 0L);
                // 已经存在
                // 1. 若处于爬取中，则等待爬取结束
                if (stepManager.TryGetValue(followerId, out var crawStep))
                {
                    if (!crawStep.ExistsCrawlerTaskId(crawlerTaskParams.CrawlerTaskId))
                    {
                        // 添加到爬取任务中
                        crawStep.AddParent(this);
                        await crawStep.ExecuteTask;
                        crawStep.RemoveParent(this);
                    }
                }
                else
                {                    
                    // 保存粉丝信息
                    var followersCrawler = new FollowersStep(crawlerTaskParams, authorInfo.Id, followerInfo, followerId);
                    followersCrawler.AddParent(this);
                    stepManager.TryAdd(followerId, followersCrawler);
                    await followersCrawler.StartAsync();
                    stepManager.TryRemove(followerId, out _);
                    followersCrawler.RemoveParent(this);
                }

                followerInfo = await followersGetter.Next();
            }
        }
    }
}
