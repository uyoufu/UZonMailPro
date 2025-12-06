
using log4net;
using Newtonsoft.Json.Linq;
using UZonMail.Utils.Json;
using UZonMail.ProPlugin.Services.Crawlers.ByteDance.APIs;

namespace UZonMail.ProPlugin.Services.Crawlers.TikTok
{
    public class RecommendAuthorGetter(CrawlerTaskParams taskParams) : RemoteItemListGetter
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RecommendAuthorGetter));

        protected override async Task PullNextPage()
        {
            // 更新推荐列表
            ResetPage();

            var jsonResult = await new GetRecommendList()
               .WithNeededQueries()
               .WithBogus()
               .WithHttpClient(taskParams.HttpClient)
               .GetJsonAsync();

            // 提取数据
            if (jsonResult == null) return;

            var itemList = jsonResult.SelectTokenOrDefault<List<JObject>>("itemList", []);
            if (itemList!.Count == 0)
            {
                _logger.Warn("获取推荐列表失败");
                return;
            }

            UpdatePage(int.MaxValue, itemList);
        }
    }
}
