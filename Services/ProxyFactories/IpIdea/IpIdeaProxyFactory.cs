using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.ProPlugin.Services.License;

namespace UzonMail.ProPlugin.Services.ProxyFactories.IpIdea
{
    public class IpIdeaProxyFactory : IProxyFactory
    {
        public string Kind => "ipidea";

        public int Order => 0;

        public bool CanHandle(Uri uri) =>
            uri.AbsolutePath != "/"
            && uri.Host.Equals("api.proxy.ipidea.io", StringComparison.OrdinalIgnoreCase);

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            // 判断是否有授权
            var functionAccess = serviceProvider.GetRequiredService<LicenseAccessService>();
            if (!await functionAccess.HasDynamicProxyAccess())
                return null;

            var handler = serviceProvider.GetRequiredService<IpIdeaProxyClient>();
            handler.Update(proxy);
            return handler;
        }
    }
}
