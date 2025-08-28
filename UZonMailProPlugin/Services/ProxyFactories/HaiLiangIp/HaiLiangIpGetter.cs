using UZonMail.Utils.Http.Request;
using UZonMailProPlugin.Services.ProxyFactories.Ip2World;

namespace UZonMailProPlugin.Services.ProxyFactories.HaiLiangIp
{
    public class HaiLiangIpGetter(string url) : FluentHttpRequest(HttpMethod.Get, url)
    {        

    }
}
