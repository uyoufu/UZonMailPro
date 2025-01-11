using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Utils.Http;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailCrawlerPlugin.ByteDance.Models;
using UZonMailCrawlerPlugin.ByteDance.Signer;
using UZonMailCrawlerPlugin.ByteDance.Utils;
using UZonMailCrawlerPlugin.Controllers.Base;
using UZonMailCrawlerPlugin.Utils;

namespace UZonMailCrawlerPlugin.Controllers.ByteDance
{
    /// <summary>
    /// 抖音 api 控制器
    /// </summary>
    public class TiktokApiController() : ControllerBasePro
    {
        /// <summary>
        /// 测试接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ResponseResult<JObject>> Test()
        {
            var client = new RestClient("https://www.tiktok.com/api/recommend/item_list/?WebIdLastTime=1735510869&aid=1988&app_language=en&app_name=tiktok_web&browser_language=zh-CN&browser_name=Mozilla&browser_online=true&browser_platform=Win32&browser_version=5.0%20%28Windows%20NT%2010.0%3B%20Win64%3B%20x64%29%20AppleWebKit%2F537.36%20%28KHTML%2C%20like%20Gecko%29%20Chrome%2F131.0.0.0%20Safari%2F537.36%20Edg%2F131.0.0.0&channel=tiktok_web&clientABVersions=71960613%2C70508271%2C72923695%2C73038833%2C73119040%2C73167671%2C73181848%2C73195684%2C73197618%2C73198009%2C73204428%2C73216053%2C73234258%2C73242625%2C73242630%2C70405643%2C71057832%2C71200802%2C72258247%2C73004916%2C73171280%2C73208420&cookie_enabled=true&count=12&coverFormat=2&data_collection_enabled=true&device_id=7453962167239935505&device_platform=web_pc&device_type=web_h264&focus_state=false&from_page=fyp&history_len=3&isNonPersonalized=false&is_fullscreen=false&is_page_visible=true&itemID=&language=en&odinId=7453963230416684052&os=windows&priority_region=&pullType=2&referer=&region=TW&screen_height=1080&screen_width=1920&showAboutThisAd=true&showAds=false&tz_name=Asia%2FShanghai&vv_count_fyp=81&watchLiveLastTime=&webcast_language=en&msToken=GEAv7==TGY=oUnrHGqwWMyOvoS4nMVU9rZMIiVUlwQKamdTDIKvQFlWZkm75p6oatiOCgc51ZyDq8HiTH4=eN6cQn3Xt6ffBakc7PgRPfl6&X-Bogus=DFSzswSLFSkANad0t8g/K09WcBJ/&_signature=_02B4Z6wo00001RWjhyQAAIDBIodDe7JpucEVo4OAACIb71");

            //var client = new RestClient(testApi)
            //{
            //    Timeout = -1
            //};
            var request = new RestRequest(Method.GET);
            request.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
            request.AddHeader("sec-ch-ua-mobile", "?0");
            client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
            request.AddHeader("sec-ch-ua-platform", "Windows");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Sec-Fetch-Site", "same-origin");
            request.AddHeader("Sec-Fetch-Mode", "cors");
            request.AddHeader("Sec-Fetch-Dest", "empty");
            request.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            //request.AddHeader("Host", "www.tiktok.com");
            //request.AddHeader("Connection", "keep-alive");
            IRestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);

            // 请求网络
            return JObject.Parse(response.Content).ToSuccessResponse();
        }
    }
}
