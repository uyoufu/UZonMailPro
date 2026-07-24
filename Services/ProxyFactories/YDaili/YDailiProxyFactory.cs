using Microsoft.Extensions.DependencyInjection;
using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.Services.SendCore.Proxies.Clients;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.ProPlugin.Services.License;

namespace UzonMail.ProPlugin.Services.ProxyFactories.YDaili
{
    public class YDailiProxyFactory() : IProxyFactory
    {
        public string Kind => "ydaili";

        public int Order => 0;

        public bool CanHandle(Uri uri) => IsProviderHost(uri, "ydaili.cn");

        public async Task<IProxyHandler?> CreateProxy(IServiceProvider serviceProvider, Proxy proxy)
        {
            // 判断是否有授权
            var functionAccess = serviceProvider.GetRequiredService<LicenseAccessService>();
            if (!await functionAccess.HasDynamicProxyAccess())
                return null;

            var handler = serviceProvider.GetRequiredService<YDailiProxyClient>();
            handler.Update(proxy);
            return handler;
        }

        private static bool IsProviderHost(Uri uri, string domain) =>
            uri.AbsolutePath != "/"
            && (
                uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase)
            );
    }
}
