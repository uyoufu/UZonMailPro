
namespace UZonMail.ProPlugin.Services.Crawlers.TikTok
{
    /// <summary>
    /// RootStep 没有 Key，表示根节点
    /// </summary>
    /// <param name="crawlerTaskId"></param>
    public class RootStep : CrawlStep
    {
        public RootStep(long crawlerTaskId)
        {
            AddCrawlerTaskId(crawlerTaskId);
        }

        protected override async Task ExecuteAsync()
        {
            return;
        }
    }
}
