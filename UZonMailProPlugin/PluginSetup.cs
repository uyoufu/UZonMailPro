using Uamazing.Utils.Plugin;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web;
using UZonMailProPlugin.Services.EmailBodyDecorators;
using UZonMailProPlugin.SQL;

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

            // 添加数据库上下文
            services.AddSqlContext<SqlContextPro, PostgreSqlContextPro, MySqlContextPro, SqLiteContextPro>(webApplicationBuilder.Configuration);

            // 批量注册服务
            services.AddServices();
        }

        public void UseApp(WebApplication webApplication)
        {

        }
    }
}
