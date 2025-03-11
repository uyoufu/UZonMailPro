using System.Collections.Concurrent;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.Crawlers.TikTok
{
    /// <summary>
    /// 缓存爬虫步骤
    /// </summary>
    public class CrawlStepManager : ConcurrentDictionary<long, CrawlStep>, ISingletonService
    {
    }
}
