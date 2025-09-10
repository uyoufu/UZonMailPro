using UZonMail.Core.Services.SendCore.Proxies.ProxyTesters;
using UZonMail.Utils.Http.Request;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.ProxyTesters
{
    /// <summary>
    /// 基于 https://www.223434.xyz:2234/inspection/ip 实现的 IP 查询
    /// </summary>
    public class UamazingProxyTester(HttpClient httpClient, LicenseAccessService licenseAccess) : JsonParser(httpClient,ProxyZoneType.Default)
    {
        private readonly string _apiUrl = "https://www.223434.xyz:2234/inspection/ip?accessToken=60331df07090662a84c8be1a";

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
