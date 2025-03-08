using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Json;

namespace UZonMailProPlugin.Services.Crawlers.TiTok
{
    /// <summary>
    /// 爬取粉丝数量
    /// </summary>
    /// <param name="crawlerTaskParams"></param>
    public class FollowersStep(CrawlerTaskParams crawlerTaskParams, long followingId, JObject followerInfo, long followerId) : RecommendStep(crawlerTaskParams, followerInfo, followerId)
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(FollowersStep));

        private readonly SqlContext _db = crawlerTaskParams.ServiceProvider.GetRequiredService<SqlContext>();

        protected override async Task ExecuteAsync()
        {
            // 解析粉丝信息
            var authorInfo = followerInfo.SelectTokenOrDefault<TiktokAuthor>("user");
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

            _logger.Debug($"保存粉丝 {authorInfo.Nickname}");

            // 保存作者信息
            authorInfo.FollowingAuthorId = followingId;
            await _db.TiktokAuthors.AddAsync(authorInfo);
            await _db.SaveChangesAsync();

            // 记录统计信息
            var statsInfo = followerInfo.SelectTokenOrDefault<TikTokAuthStats>("stats");
            statsInfo?.SetTo(authorInfo);

            // 解析账号
            var resolver = new SignatureResolver(authorInfo.Signature);
            resolver?.ResolveFor(authorInfo);
            await _db.SaveChangesAsync();

            // 记录爬取结果
            await SaveCrawlerTaskResult(_db, authorInfo);
        }
    }
}
