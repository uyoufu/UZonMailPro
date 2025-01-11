using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.Crawlers
{
    /// <summary>
    /// TikTok 邮箱爬虫
    /// </summary>
    public class TikTokEmailCrawler(SqlContext sqlContext, FunctionAccessService accessService, CrawlerManager crawlerManager)
        : ClawerTaskService(sqlContext, crawlerManager)
    {
        private CrawlerTaskInfo _crawlerTaskInfo;
        public void SetCrawlerTaskInfo(CrawlerTaskInfo crawlerTaskInfo)
        {
            _crawlerTaskInfo = crawlerTaskInfo;
        }

        /// <summary>
        /// 执行具体的爬取任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 判断是否有权限调用
            var hasAccess = await accessService.HasTiktokEmailCrawlerAccess();
            if (!hasAccess) return;

            // 开始执行爬虫任务
            await StartCrawler();
        }

        private async Task StartCrawler()
        {
            // 开始爬取
        }
    }
}
