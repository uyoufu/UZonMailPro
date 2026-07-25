using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.EmailCrawler;
using UzonMail.Utils.Json;

namespace UzonMail.ProPlugin.Services.Crawlers.TikTok
{
    /// <summary>
    /// 爬取粉丝数量
    /// </summary>
    /// <param name="crawlerTaskParams"></param>
    public class FollowersStep : RecommendStep
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FollowersStep));
        private readonly CrawlerTaskParams _crawlerTaskParams;
        private readonly long _followingId;
        private readonly JObject _followerInfo;

        private readonly SqlContextPro _db;

        public FollowersStep(
            CrawlerTaskParams crawlerTaskParams,
            long followingId,
            JObject followerInfo,
            long followerId
        )
            : base(crawlerTaskParams, followerInfo, followerId)
        {
            _crawlerTaskParams = crawlerTaskParams;
            _followingId = followingId;
            _followerInfo = followerInfo;
            _db = crawlerTaskParams.ServiceProvider.GetRequiredService<SqlContextPro>();
        }

        protected override async Task ExecuteAsync()
        {
            // 解析粉丝信息
            var authorInfo = _followerInfo.SelectTokenOrDefault<TiktokAuthor>("user");
            if (authorInfo == null)
                return;

            // 保存作者信息
            await SaveAuthor(authorInfo);

            // 爬取粉丝
            await CrawlFolloers(authorInfo);

            // 移除缓存
            _crawlerTaskParams.StepManager.TryRemove(authorInfo.Id, out _);
        }

        private async Task SaveAuthor(TiktokAuthor authorInfo)
        {
            // 判断是否存在，若存在，则不再保存
            var existOne = await _db.TiktokAuthors.FirstOrDefaultAsync(x => x.Id == authorInfo.Id);
            if (existOne != null)
                return;

            _logger.Debug($"保存粉丝 {authorInfo.Nickname}");

            // 保存作者信息
            authorInfo.FollowingAuthorId = _followingId;
            await _db.TiktokAuthors.AddAsync(authorInfo);
            await _db.SaveChangesAsync();

            // 记录统计信息
            var statsInfo = _followerInfo.SelectTokenOrDefault<TikTokAuthStats>("stats");
            statsInfo?.SetTo(authorInfo);

            // 解析账号
            if (!string.IsNullOrEmpty(authorInfo.Signature))
                new SignatureResolver(authorInfo.Signature).ResolveFor(authorInfo);
            await _db.SaveChangesAsync();

            // 记录爬取结果
            await SaveCrawlerTaskResult(_db, authorInfo);
        }
    }
}
