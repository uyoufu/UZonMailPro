using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UZonMail.Utils.Json;
using UZonMailProPlugin.Services.Crawlers.ByteDance.APIs;

namespace UZonMailProPlugin.Services.Crawlers.TikTok
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

        public override void Reset()
        {
            _minCursor = 0;
            base.Reset();
        }

        protected override async Task PullNextPage()
        {
            // 重置
            Reset();

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

            UpdatePage(total, userList);
        }
    }
}
