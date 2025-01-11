using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UZonMail.Utils.Extensions;
using UZonMail.Utils.Json;
using UZonMailCrawlerPlugin.Utils;

namespace UZonMailCrawlerPlugin.ByteDance.Models
{
    public class TiktokItemListQueryModel(int aid) : BaseJsonModel
    {
        public long WebIdLastTime { get; set; } = DateTime.Now.ToTimestamp() / 1000;
        public int Aid { get; set; } = aid;
        public I18nLanguage AppLanguage { get; set; } = I18nLanguage.en;
        public AppName AppName { get; set; } = AppName.TikTokWeb;
        public I18nLanguage BrowserLanguage { get; set; } = I18nLanguage.zhCN;
        public string BrowserName { get; set; } = "Mozilla";
        public bool BrowserOnline { get; set; } = true;
        public string BrowserPlatform { get; set; } = "Win32";
        public string BrowserVersion { get; set; } = UserAgent.GetBrowserVersion();
        public TikTokChannel Channel { get; set; } = TikTokChannel.TikTokWeb;
        public string ClientABVersions { get; set; } = "71960613,70508271,72437276,72920976,72923695,72961679,73038833,73119040,73122399,73167671,73179188,73197618,73198009,73204428,73216053,73230562,73234258,73242625,73242628,73242630,70138197,70156809,70405643,71057832,71200802,71381811,71516509,71803300,71962127,72258247,72360691,72408100,72854054,72892778,73004916,73171280,73208420,73233984";
        public bool CookieEnabled { get; set; } = false;
        public int Count { get; set; } = 12;
        public int CoverFormat { get; set; } = 2;
        public bool DataCollectionEnabled { get; set; } = true;
        public long DeviceId { get; set; } = 7453962167239935504;
        public DevicePlatform DevicePlatform { get; set; } = DevicePlatform.WebPC;
        public DeviceType DeviceType { get; set; } = DeviceType.WebH264;
        public bool FocusState { get; set; } = false;
        public string FromPage { get; set; } = "fyp";
        public int HistoryLen { get; set; } = 6;
        public bool IsNonPersonalized { get; set; }
        public bool IsFullscreen { get; set; }
        public bool IsPageVisible { get; set; } = true;
        public string ItemID { get; set; } = string.Empty;
        public I18nLanguage Language { get; set; } = I18nLanguage.en;
        public OSType Os { get; set; } = OSType.Windows;
        public RegionName PriorityRegion { get; set; } = RegionName.US;
        public int PullType { get; set; } = 1;
        public string Referer { get; set; } = string.Empty;
        public RegionName Region { get; set; } = RegionName.US;
        public int ScreenHeight { get; set; } = 1080;
        public int ScreenWidth { get; set; } = 1920;
        public bool ShowAboutThisAd { get; set; } = true;
        public bool ShowAds { get; set; } = true;
        public string TzName { get; set; } = "Asia/Shanghai";
        [JsonProperty("verifyFp")]
        public string VeryfyFp { get; set; } = "verify_m5akn57q_1tE6nw5n_GcBI_4ucN_Bi6m_7fyPBwZe0jhp";
        public int VvCountFyp { get; set; } = 147;
        public long? WatchLiveLastTime { get; set; } = null;
        public I18nLanguage WebcastLanguage { get; set; } = I18nLanguage.en;

        // 下面的字段在其它地方定义，暂时注释掉
        //public string MsToken { get; set; }
        //[JsonProperty("X-Bogus")]
        //public string XBogus { get; set; }
        //[JsonProperty("_signature")]
        //public string Signature { get; set; }

        public string ToRequestUrl(string baseApi)
        {
            var json = this.ToSnakeCaseJson();
            var jobj = JObject.Parse(json);
            var query = jobj.ToQueryString();
            return $"{baseApi}?{query}";
        }
    }
}
