using System.Collections.Concurrent;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Services.Crawlers.TikTok
{
    /// <summary>
    /// 缓存爬虫步骤
    /// </summary>
    public class CrawlStepManager : ConcurrentDictionary<long, CrawlStep>, ISingletonService { }
}
