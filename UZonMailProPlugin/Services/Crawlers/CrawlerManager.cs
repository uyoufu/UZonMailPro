using System.Collections.Concurrent;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.Crawlers
{
    /// <summary>
    /// 爬虫管理器
    /// </summary>
    public class CrawlerManager(IServiceProvider serviceProvider, ILogger<CrawlerManager> logger) : ConcurrentDictionary<long, ClawerTaskService>, ISingletonService
    {
        /// <summary>
        /// 开始 TikTok 邮箱爬虫
        /// 若爬虫任务不存在，则创建一个新的爬虫任务
        /// 若存在，则直接返回
        /// </summary>
        /// <param name="crawlerTaskInfo"></param>
        public void StartTikTokEmailCrawler(CrawlerTaskInfo crawlerTaskInfo)
        {
            if (ContainsKey(crawlerTaskInfo.Id))
            {
                logger.LogWarning($"TikTok 邮箱爬虫任务 {crawlerTaskInfo.Id} 已经存在，无需重复创建");
                return;
            }

            var scope = serviceProvider.CreateAsyncScope();
            // 创建爬虫任务
            var crawlerTaskService = scope.ServiceProvider.GetRequiredService<TikTokEmailCrawler>();
            crawlerTaskService.SetCrawlerTaskInfo(crawlerTaskInfo);
            _ = crawlerTaskService.StartAsync(scope,crawlerTaskInfo.Id);
        }

        /// <summary>
        /// 停止爬虫任务
        /// </summary>
        /// <param name="crawlerTaskId"></param>
        /// <returns></returns>
        public async Task StopCrawler(long crawlerTaskId)
        {
            if (!TryGetValue(crawlerTaskId, out var crawler)) return;
            await crawler.StopAsync();
        }
    }
}
