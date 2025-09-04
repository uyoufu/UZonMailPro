using UZonMail.Utils.Http.Request;

namespace UZonMailProPlugin.Services.ProxyFactories.HaiLiangIp
{
    public class HaiLiangIpGetter(string url) : FluentHttpRequest(HttpMethod.Get, url)
    {
        public HaiLiangIpGetter WithSocks5()
        {
            var protocolParameter = GetParameter("protocol");
            var protocalValue = protocolParameter == null ? "socks5" : protocolParameter.Value;

            AddQuery("type", protocalValue == "http" ? "1":"2");
            if (protocolParameter == null)
            {
                AddQuery("protocol", protocalValue);
            }
            return this;
        }
    }
}
