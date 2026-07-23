namespace UZonMail.ProPlugin.Services.Crawlers.TikTok
{
    public class CrawlerTaskParams
    {
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 爬虫管理器
        /// </summary>
        public CrawlStepManager StepManager { get; set; }

        public HttpClient HttpClient { get; set; }

        public long CrawlerTaskId { get; set; }

        /// <summary>
        /// 广告 Id
        /// </summary>
        public string OdinId { get; set; }

        /// <summary>
        /// 设备 ID
        /// </summary>
        public string DeviceId { get; set; }
    }
}
