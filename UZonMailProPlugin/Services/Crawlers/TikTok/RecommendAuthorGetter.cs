
using Newtonsoft.Json.Linq;
using UZonMail.Utils.Json;
using UZonMailProPlugin.Services.Crawlers.ByteDance.APIs;

namespace UZonMailProPlugin.Services.Crawlers.TiTok
{
    public class RecommendAuthorGetter(CrawlerTaskParams taskParams) : RemoteItemListGetter
    {
        protected override async Task PullNextPage()
        {
            var jsonResult = await new GetRecommendList()
               .WithNeededQueries()
               .WithBogus()
               .WithHttpClient(taskParams.HttpClient)
               .GetJsonAsync();

            // 提取数据
            if (jsonResult == null) return;

            var itemList = jsonResult.SelectTokenOrDefault<List<JObject>>("itemList", []);
            if (itemList!.Count == 0) return;

            UpdatePage(int.MaxValue, itemList);
        }
    }
}
