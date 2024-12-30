using Uamazing.Utils.Plugin;
using UZonMail.Utils.Web;
namespace UZonMailCrawlerPlugin
{
    public class PluginSetup : IPlugin
    {
        public void UseApp(WebApplication webApplication)
        {
            
        }

        public void UseServices(WebApplicationBuilder webApplicationBuilder)
        {
            var services = webApplicationBuilder.Services;
            // 批量注册服务
            services.AddServices();
        }
    }
}
