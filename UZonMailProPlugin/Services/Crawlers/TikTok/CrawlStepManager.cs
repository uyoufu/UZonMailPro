using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.Services.Crawlers.ByteDance.APIs;

namespace UZonMailProPlugin.Services.Crawlers.TiTok
{
    /// <summary>
    /// 缓存爬虫步骤
    /// </summary>
    public class CrawlStepManager : ConcurrentDictionary<long, CrawlStep>, ISingletonService
    {
    }
}
