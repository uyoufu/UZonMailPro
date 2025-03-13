
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Services.HostedServices;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.EmailCrawler;

namespace UZonMailProPlugin.Services.Crawlers
{
    /// <summary>
    /// 应该在数据库迁移成功后，再启动爬虫任务
    /// </summary>
    /// <param name="ssf"></param>
    /// <param name="crawlerManager"></param>
    public class CrawlerTaskRestorer(IServiceScopeFactory ssf, CrawlerManager crawlerManager) : IHostedServiceStart, IScopedService<IHostedServiceStart>
    {
        // 靠后启动
        public int Order => 100;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = ssf.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<SqlContextPro>();
            // 对爬虫任务进行恢复
            var runnintTasks = await db.CrawlerTaskInfos
                .Where(x => x.Status == CrawlerStatus.Running)
                .ToListAsync(cancellationToken: stoppingToken);

            foreach (var task in runnintTasks)
            {
                crawlerManager.StartTikTokEmailCrawler(task);
            }
        }
    }
}
