using Uamazing.Utils.Email;
using Uamazing.Utils.Plugin;
using Uamazing.Utils.Web.Token;
using UZonMail.Utils.Web;
using UZonMailProPlugin.Services.Token;
using UZonMailProPlugin.Services.EmailBodyDecorators;
using UZonMail.Core.Database.Updater;

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

            // 添加 TokenPayloadsBuilder
            TokenClaimsBuilders.AddBuilder(new TokenClaimsBuilder());

            // 添加邮件正文装饰器
            EmailBodyDecorators.AddDecorator(new EmailTrackerDecoractor());
            EmailBodyDecorators.AddDecorator(new UnsubesribeButtonDecorator());

            // 添加更新器
            DataUpdaterManager.AddCallingAssembly();
        }

        public void UseApp(WebApplication webApplication)
        {

        }
    }
}
