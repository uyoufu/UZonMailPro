using System.Collections.Concurrent;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.Services.Crawlers.TikTok;
using UZonMailProPlugin.SQL.EmailCrawler;

namespace UZonMailProPlugin.Services.Crawlers
{
    /// <summary>
    /// 爬虫管理器
    /// </summary>
    public class CrawlerManager(IServiceProvider serviceProvider, ILogger<CrawlerManager> logger) : ConcurrentDictionary<long, CrawlerTaskBase>, ISingletonService
    {
        /// <summary>
        /// 开始 TikTok 邮箱爬虫
        /// 若爬虫任务不存在，则创建一个新的爬虫任务
        /// 若存在，则直接返回
        /// </summary>
        /// <param name="crawlerTaskInfo"></param>
        public async Task StartTikTokEmailCrawler(CrawlerTaskInfo crawlerTaskInfo)
        {
            try
            {
                if (TryGetValue(crawlerTaskInfo.Id, out var value))
                {
                    logger.LogWarning($"TikTok 邮箱爬虫任务 {crawlerTaskInfo.Id} 已经存在，重新激活");
                    await value.RestartAsync(crawlerTaskInfo.Id);
                    return;
                }
                else
                {
                    using var scope = serviceProvider.CreateAsyncScope();
                    var crawlerTaskId = crawlerTaskInfo.Id;
                    // 创建爬虫任务
                    var crawlerTaskService = scope.ServiceProvider.GetRequiredService<TikTokEmailCrawler>();
                    TryAdd(crawlerTaskId, crawlerTaskService);
                    await crawlerTaskService.StartAsync(scope, crawlerTaskId);
                    TryRemove(crawlerTaskId, out _);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"TikTok 邮箱爬虫任务 {crawlerTaskInfo.Id} 执行失败");
            }
        }

        /// <summary>
        /// 停止爬虫任务
        /// </summary>
        /// <param name="crawlerTaskId"></param>
        /// <returns></returns>
        public async Task StopCrawler(long crawlerTaskId)
        {
            if (!TryGetValue(crawlerTaskId, out var crawler)) return;
            await crawler.StopAsync(crawlerTaskId);
        }
    }
}
