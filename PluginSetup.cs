using UzonMail.DB.SQL;
using UzonMail.ProPlugin.SQL;
using UzonMail.Utils.Plugin;
using UzonMail.Utils.Web;

namespace UzonMail.ProPlugin
{
    /// <summary>
    /// 加载插件
    /// </summary>
    public class PluginSetup : IPlugin
    {
        public int Priority => 1;

        public void ConfigureServices(IHostApplicationBuilder hostBuilder)
        {
            var services = hostBuilder.Services;

            // 添加数据库上下文
            services.AddSqlContext<SqlContextPro, PostgreSqlContextPro, SqLiteContextPro>(
                hostBuilder.Configuration
            );

            // 批量注册服务
            services.AddServices();
        }

        public void ConfigureApp(IApplicationBuilder app) { }
    }
}
