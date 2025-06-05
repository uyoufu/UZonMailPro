using UZonMail.Utils.Http.Request;
using UZonMailProPlugin.Services.ProxyFactories.IPFoxy;

namespace UZonMailProPlugin.Services.ProxyFactories.IpIdea
{
    // 参考 https://www.ipidea.net/getapi/
    // 完整链接示例：http://api.proxy.ipidea.io/getBalanceProxyIp?num=100&return_type=txt&lb=1&sb=0&flow=1&regions=&protocol=socks5
    public class IpIdeaProxyGetter(string url) : FluentHttpRequest(HttpMethod.Get, url)
    {
        public IpIdeaProxyGetter WithIPNumber(int number = 10)
        {
            AddQuery("num", number.ToString());
            return this;
        }

        public IpIdeaProxyGetter WithJsonReturnType()
        {
            // Format:TXT JSON
            AddQuery("return_type", "json");
            return this;
        }

        /// <summary>
        /// 指定运营商
        /// 非必须
        /// </summary>
        /// <param name="isp"></param>
        /// <returns></returns>
        public IpIdeaProxyGetter WithSocks5()
        {
            AddQuery("protocol", "socks5");
            return this;
        }
    }
}
