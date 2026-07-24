using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.ProPlugin.Services.License;
using UzonMail.ProPlugin.Services.ProxyFactories.YDaili;

namespace UzonMail.ProPlugin.Services.ProxyFactories.Ip2World
{
    public class Ip2WorldProxyFactory : IProxyFactory
    {
        public string Kind => "ip2world";

        public int Order => 0;

        public bool CanHandle(Uri uri) =>
            uri.AbsolutePath != "/"
            && (
                uri.Host.Equals("ip2world.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(".ip2world.com", StringComparison.OrdinalIgnoreCase)
            );

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            // 判断是否有授权
            var functionAccess = serviceProvider.GetRequiredService<LicenseAccessService>();
            if (!await functionAccess.HasDynamicProxyAccess())
                return null;

            var handler = serviceProvider.GetRequiredService<Ip2WorldProxyClient>();
            handler.Update(proxy);
            return handler;
        }
    }
}
