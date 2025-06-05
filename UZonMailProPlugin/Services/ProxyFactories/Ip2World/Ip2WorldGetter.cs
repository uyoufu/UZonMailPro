using UZonMail.Utils.Http.Request;
using UZonMailProPlugin.Services.ProxyFactories.YDaili;

namespace UZonMailProPlugin.Services.ProxyFactories.Ip2World
{
    public class Ip2WorldGetter(string url) : FluentHttpRequest(HttpMethod.Get, url)
    {
        public Ip2WorldGetter WithIPNumber(int number = 10)
        {
            AddQuery("num", number.ToString());           
            return this;
        }

        public Ip2WorldGetter WithJsonReturnType()
        {
            // Format:TXT JSON
            AddQuery("return_type", "json");
            return this;
        }

        public Ip2WorldGetter WithHttpsProtocol()
        {
            AddQuery("protocol", "https");
            return this;
        }

        /// <summary>
        /// 指定运营商
        /// 非必须
        /// </summary>
        /// <param name="isp"></param>
        /// <returns></returns>
        public Ip2WorldGetter WithDelimiter()
        {
            // Delimiters(1:\r\n 2:/ br 3:\r 4:\n 5:\t)
            AddQuery("lb", "4");
            return this;
        }
    }
}
