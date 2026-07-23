using UZonMail.Utils.Http.Request;

namespace UZonMail.ProPlugin.Services.ProxyFactories.IPFoxy
{
    public class IpFoxyGetter(string url) : FluentHttpRequest(HttpMethod.Get, url)
    {
        public IpFoxyGetter WithIPNumber(int number = 10)
        {
            AddQuery("count", number.ToString());
            return this;
        }

        public IpFoxyGetter WithJsonReturnType()
        {
            // Format:TXT JSON
            AddQuery("type", "json");
            return this;
        }
       
        /// <summary>
        /// 指定运营商
        /// 非必须
        /// </summary>
        /// <param name="isp"></param>
        /// <returns></returns>
        public IpFoxyGetter WithPeriod()
        {
            // 0 - 代理失效后，返回新地址
            // 1 - 每次请求返回新地址
            AddQuery("period", "1");
            return this;
        }
    }
}
