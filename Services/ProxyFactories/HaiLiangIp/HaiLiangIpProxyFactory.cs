using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.ProPlugin.Services.License;
using UzonMail.ProPlugin.Services.ProxyFactories.Ip2World;

namespace UzonMail.ProPlugin.Services.ProxyFactories.HaiLiangIp
{
    public class HaiLiangIpProxyFactory : IProxyFactory
    {
        public string Kind => "hailiangip";

        public int Order => 0;

        public bool CanHandle(Uri uri) =>
            uri.AbsolutePath != "/"
            && (
                uri.Host.Equals("hailiangip.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(".hailiangip.com", StringComparison.OrdinalIgnoreCase)
            );

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
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
