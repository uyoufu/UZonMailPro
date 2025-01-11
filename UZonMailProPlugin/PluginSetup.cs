using Uamazing.Utils.Plugin;
using UZonMail.Utils.Web;
using UZonMailProPlugin.Services.EmailDecorators;

namespace UZonMailProPlugin
{
    /// <summary>
    /// 加载插件
    /// </summary>
    public class PluginSetup : IPlugin
    {
        public void UseServices(WebApplicationBuilder webApplicationBuilder)
        {
            var services = webApplicationBuilder.Services;
            // 批量注册服务
            services.AddServices();
        }

        public void UseApp(WebApplication webApplication)
        {

        }
    }
}
