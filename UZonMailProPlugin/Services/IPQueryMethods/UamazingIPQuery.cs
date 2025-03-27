using UZonMail.Core.Services.IPQueryMethods;
using UZonMail.Utils.Http.Request;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.IPQueryMethods
{
    /// <summary>
    /// 基于 http://httpbin.org/ip 实现的 IP 查询
    /// </summary>
    public class UamazingIPQuery(HttpClient httpClient, LicenseAccessService licenseAccess) : JsonParser(httpClient)
    {
        private readonly string _apiUrl = "http://httpbin.org/ip";

        /// <summary>
        /// 最优先
        /// </summary>
        public override int Order { get; } = -100;

        protected override FluentHttpRequest GetHttpRequestWithoutProxy()
        {
            return new FluentHttpRequest(HttpMethod.Get, _apiUrl);
        }

        protected override string GetJsonPathOfIP()
        {
            return "ip";
        }

        /// <summary>
        /// 只有专业版本以上才能使用
        /// </summary>
        /// <returns></returns>
        protected override async Task<bool> Validate()
        {
            var hashPro = await licenseAccess.HasProLicense();
            if (!hashPro)
            {
                return false;
            }
            return await base.Validate();
        }
    }
}
