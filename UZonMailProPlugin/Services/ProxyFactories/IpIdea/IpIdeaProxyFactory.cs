using UZonMail.Core.Services.SendCore.Proxies;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.Services.ProxyFactories.IPFoxy;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Core.Services.SendCore.Proxies.Clients;

namespace UZonMailProPlugin.Services.ProxyFactories.IpIdea
{
    public class IpIdeaProxyFactory : IProxyFactory
    {
        public int Order => 0;

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            if (!proxy.Url.Contains("api.proxy.ipidea.io")) return null;

            // 判断是否有授权
            var functionAccess = serviceProvider.GetRequiredService<LicenseAccessService>();
            if (!await functionAccess.HasDynamicProxyAccess()) return null;

            var handler = serviceProvider.GetRequiredService<IpFoxyProxyClient>();
            handler.Update(proxy);
            return handler;
        }
    }
}
