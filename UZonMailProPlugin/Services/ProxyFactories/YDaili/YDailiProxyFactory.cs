using Microsoft.Extensions.DependencyInjection;
using UZonMail.CorePlugin.Services.SendCore.Proxies;
using UZonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.ProPlugin.Services.License;

namespace UZonMail.ProPlugin.Services.ProxyFactories.YDaili
{
    public class YDailiProxyFactory() : IProxyFactory
    {
        public int Order => 0;

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            if (!proxy.Url.Contains("ydaili.cn"))
                return null;

            // 判断是否有授权
            var functionAccess = serviceProvider.GetRequiredService<LicenseAccessService>();
            if (!await functionAccess.HasDynamicProxyAccess())
                return null;

            var handler = serviceProvider.GetRequiredService<YDailiProxyClient>();
            handler.Update(proxy);
            return handler;
        }
    }
}
