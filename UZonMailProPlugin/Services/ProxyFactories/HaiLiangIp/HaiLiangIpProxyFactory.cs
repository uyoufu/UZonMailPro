using UZonMail.CorePlugin.Services.SendCore.Proxies;
using UZonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.ProPlugin.Services.License;
using UZonMail.ProPlugin.Services.ProxyFactories.Ip2World;

namespace UZonMail.ProPlugin.Services.ProxyFactories.HaiLiangIp
{
    public class HaiLiangIpProxyFactory : IProxyFactory
    {
        public int Order => 0;

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            if (!proxy.Url.Contains("hailiangip.com"))
                return null;

            // 判断是否有授权
            var functionAccess = serviceProvider.GetRequiredService<LicenseAccessService>();
            if (!await functionAccess.HasDynamicProxyAccess())
                return null;

            var handler = serviceProvider.GetRequiredService<HaiLiangIpProxyClient>();
            handler.Update(proxy);
            return handler;
        }
    }
}
