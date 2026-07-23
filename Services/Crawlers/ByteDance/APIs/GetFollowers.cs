using UZonMail.Utils.Http;
using UZonMail.ProPlugin.Modules.ByteDance.Signer;

namespace UZonMail.ProPlugin.Services.Crawlers.ByteDance.APIs
{
    public class GetFollowers() : ApiBase(HttpMethod.Get, $"https://www.tiktok.com/api/user/list/")
    {
        private int _countPerPage = 30;

        public GetFollowers WithNeededQueries()
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
            AddQuery("cookie_enabled", "true");
            AddQuery("count", _countPerPage.ToString());
            AddQuery("data_collection_enabled", "true");
            //AddQuery("device_id", "7453962167239935505");
            AddQuery("device_platform", "web_pc");
            AddQuery("focus_state", "false");
            AddQuery("from_page", "user");
            AddQuery("history_len", _countPerPage.ToString());
            AddQuery("is_fullscreen", "false");
            AddQuery("is_page_visible", "true");
            AddQuery("maxCursor", "0");
            //AddQuery("minCursor", "0");
            AddQuery("os", "windows");
            AddQuery("priority_region", "");
            AddQuery("region", "TW");
            AddQuery("screen_height", "1080");
            AddQuery("screen_width", "1920");
            AddQuery("tz_name", "Asia/Shanghai");
            AddQuery("webcast_language", "en");
            AddQuery("msToken", ByteDanceCredentials.GetMsToken());
            return this;
        }

        public GetFollowers WithDeviceId(string deviceId)
        {
            AddQuery("device_id", deviceId);
            return this;
        }

        public GetFollowers WithSecUid(string secUid)
        {
            AddQuery("secUid", secUid);
            return this;
        }

        public GetFollowers WithMinCursor(long minCursor)
        {
            AddQuery("minCursor", minCursor.ToString());
            return this;
        }

        public GetFollowers WithBogus()
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
