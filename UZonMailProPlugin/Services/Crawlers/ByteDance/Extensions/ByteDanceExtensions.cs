using Microsoft.Net.Http.Headers;
using UZonMail.Utils.Http;
using UZonMailProPlugin.Utils.Browser;

namespace UZonMailProPlugin.Services.Crawlers.ByteDance.Extensions
{
    public static class ByteDanceExtensions
    {
        /// <summary>
        /// 添加用户代理头部
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static HttpClient AddUserAgentHeaders(this HttpClient httpClient)
        {
            httpClient.TryAddHeader("sec-ch-ua", BrowserMock.GetSecChUa())
                .TryAddHeader("sec-ch-ua-mbile", "?0")
                .TryAddHeader("User-Agent", BrowserMock.GetChromeUserAgent())
                .TryAddHeader("sec-ch-ua-platform", "Windows")
                .TryAddHeader("Accept", "*/*")
                .TryAddHeader("Sec-Fetch-Site", "same-origin")
                .TryAddHeader("Sec-Fetch-Mode", "cors")
                .TryAddHeader("Sec-Fetch-Dest", "empty")
                .TryAddHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
            return httpClient;
        }
    }
}
