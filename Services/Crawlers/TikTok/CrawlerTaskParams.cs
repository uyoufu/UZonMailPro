namespace UzonMail.ProPlugin.Services.Crawlers.TikTok
{
    public class CrawlerTaskParams
    {
        public required IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 爬虫管理器
        /// </summary>
        public required CrawlStepManager StepManager { get; set; }

        public required HttpClient HttpClient { get; set; }

        public long CrawlerTaskId { get; set; }

        /// <summary>
        /// 广告 Id
        /// </summary>
        public required string OdinId { get; set; }

        /// <summary>
        /// 设备 ID
        /// </summary>
        public required string DeviceId { get; set; }
    }
}
