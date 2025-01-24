using UZonMail.Utils.Http.Request;
using UZonMail.Utils.Json;
using UZonMailProPlugin.Modules.ByteDance.Signer;
using UZonMailProPlugin.Utils.Browser;

namespace UZonMailProPlugin.Services.Crawlers.ByteDance.APIs
{
    public class GetRecommendList() : ApiBase(HttpMethod.Get, $"https://www.tiktok.com/api/recommend/item_list/")
    {
        private int _countPerPage = 30;

        public GetRecommendList WithNeededQueries()
        {
            AddQuery("WebIdLastTime", "1735510869");
            AddQuery("aid", "1988");
            AddQuery("app_language", "en");
            AddQuery("app_name", "tiktok_web");
            AddQuery("browser_language", "zh-CN");
            AddQuery("browser_name", BrowserMock.GetBrowserName());
            AddQuery("browser_online", "true");
            AddQuery("browser_platform", "Win32");
            AddQuery("browser_version", BrowserMock.GetBrowserChromeVersion());
            AddQuery("channel", "tiktok_web");
            AddQuery("clientABVersions", BrowserMock.GetClientABVersions());
            AddQuery("cookie_enabled", "true");
            AddQuery("count", _countPerPage.ToString());
            AddQuery("coverFormat", "2");
            AddQuery("data_collection_enabled", "true");
            AddQuery("device_id", "7453962167239935505");
            AddQuery("device_platform", "web_pc");
            AddQuery("device_type", "web_h264");
            AddQuery("focus_state", "false");
            AddQuery("from_page", "fyp");
            AddQuery("history_len", _countPerPage.ToString());
            AddQuery("isNonPersonalized", "false");
            AddQuery("is_fullscreen", "false");
            AddQuery("is_page_visible", "true");
            AddQuery("itemID", "");
            AddQuery("language", "en");
            AddQuery("odinId", "7453963230416684052");
            AddQuery("os", "windows");
            AddQuery("priority_region", "");
            // 1-首次加载 2-下拉加载 3-重新加载
            AddQuery("pullType", "2");
            AddQuery("referer", "");
            AddQuery("region", "TW");
            AddQuery("screen_height", "1080");
            AddQuery("screen_width", "1920");
            AddQuery("showAboutThisAd", "true");
            AddQuery("showAds", "false");
            AddQuery("tz_name", "Asia/Shanghai");
            AddQuery("vv_count_fyp", "81");
            AddQuery("watchLiveLastTime", "");
            AddQuery("webcast_language", "en");
            AddQuery("msToken", ByteDanceCredentials.GetMsToken());
            return this;
        }

        public GetRecommendList WithBogus()
        {
            // 生成 xBogus
            var finnalUrl = BuildUri();
            var queryString = finnalUrl.Query.TrimStart('?');
            var xBogus = new TiktokCredentials().GetBogus(queryString);

            AddQuery("X-Bogus", xBogus);
            AddQuery("_signature", ByteDanceCredentials.GetSignature());
            return this;
        }
    }
}
