using UZonMail.Utils.Http.Request;

namespace UZonMail.ProPlugin.Services.ProxyFactories.YDaili
{
    public enum YDailiFormat
    {
        Text,
        Json
    }

    public class YDailiGetter(string url) : FluentHttpRequest(HttpMethod.Get, url)
    {
        public YDailiGetter WithIPNumber(int number = 10)
        {
            AddQuery("number", number.ToString());
            return this;
        }

        public YDailiGetter WithFormat(YDailiFormat format = YDailiFormat.Json)
        {
            AddQuery("format", format.ToString());
            return this;
        }

        public YDailiGetter WithOrderId(string orderId)
        {
            AddQuery("orderId", orderId);
            return this;
        }

        /// <summary>
        /// 指定城市
        /// 非必须
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public YDailiGetter WithCity(string city)
        {
            AddQuery("city", city);
            return this;
        }

        /// <summary>
        /// 指定省份
        /// 非必须
        /// </summary>
        /// <param name="province"></param>
        /// <returns></returns>
        public YDailiGetter WithProvince(string province)
        {
            AddQuery("province", province);
            return this;
        }


        /// <summary>
        /// 指定运营商
        /// 非必须
        /// </summary>
        /// <param name="isp"></param>
        /// <returns></returns>
        public YDailiGetter WithISP(string isp)
        {
            AddQuery("isp", isp);
            return this;
        }
    }
}
