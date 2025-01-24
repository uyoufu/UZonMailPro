
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;

namespace UZonMailProPlugin.Services.Crawlers
{
    public class CrawlerTaskRestorer(IServiceScopeFactory ssf, CrawlerManager crawlerManager) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = ssf.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
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
