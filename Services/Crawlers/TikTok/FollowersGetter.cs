using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UZonMail.Utils.Json;
using UZonMail.ProPlugin.Services.Crawlers.ByteDance.APIs;

namespace UZonMail.ProPlugin.Services.Crawlers.TikTok
{
    /// <summary>
    /// 粉丝获取器
    /// </summary>
    public class FollowersGetter(CrawlerTaskParams taskParams, string secUid) : RemoteItemListGetter
    {
        /// <summary>
        /// 传递给下一个请求的 cursor
        /// </summary>
        private long _minCursor = 0;
        private static readonly int _minFollowersCount = 1000;

        protected override async Task PullNextPage()
        {
            // 重置
            ResetPage();

            // 获取下一页粉丝数据
            var jsonResult = await new GetFollowers()
                .WithNeededQueries()
                .WithSecUid(secUid)
                .WithDeviceId(taskParams.DeviceId)
                .WithMinCursor(_minCursor)
                .WithBogus()
                .WithHttpClient(taskParams.HttpClient)
                .GetJsonAsync();

            _minCursor = jsonResult.SelectTokenOrDefault("minCursor", 0L);

            var total = jsonResult.SelectTokenOrDefault("total", 0);
            var userList = jsonResult.SelectTokenOrDefault<List<JObject>>("userList", []);

            // 若粉丝太少，则不再爬取
            if (total < _minFollowersCount)
            {
                return;
            }

            // 如果已经到最后一页了，则不再拉取
            if (userList.Count == 0)
            {
                return;
            }

            UpdatePage(total, userList);
        }
    }
}
