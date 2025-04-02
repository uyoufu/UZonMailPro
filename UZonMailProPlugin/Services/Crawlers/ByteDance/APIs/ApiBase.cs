using UZonMail.Utils.Http;
using UZonMail.Utils.Http.Request;

namespace UZonMailProPlugin.Services.Crawlers.ByteDance.APIs
{
    public abstract class ApiBase : FluentHttpRequest
    {
        public ApiBase(HttpMethod method, string url) : base(method, url)
        {
            AddNeededHeaders();
        }

        public ApiBase() 
        {
            AddNeededHeaders();
        }

        /// <summary>
        /// 添加需要的头部
        /// </summary>
        private void AddNeededHeaders()
        {
            this.AddHeader("sec-ch-ua", BrowserMock.GetSecChUa());
            this.AddHeader("sec-ch-ua-mbile", "?0");
            this.AddHeader("User-Agent", BrowserMock.GetChromeUserAgent());
            this.AddHeader("sec-ch-ua-platform", "Windows");
            this.AddHeader("Accept", "*/*");
            this.AddHeader("Sec-Fetch-Site", "same-origin");
            this.AddHeader("Sec-Fetch-Mode", "cors");
            this.AddHeader("Sec-Fetch-Dest", "empty");
            this.AddHeader("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
        }
    }
}
