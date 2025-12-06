using UZonMail.DB.SQL;
using UZonMail.ProPlugin.SQL;
using UZonMail.Utils.Plugin;
using UZonMail.Utils.Web;

namespace UZonMail.ProPlugin
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
            services.AddSqlContext<
                SqlContextPro,
                PostgreSqlContextPro,
                MySqlContextPro,
                SqLiteContextPro
            >(hostBuilder.Configuration);

            // 批量注册服务
            services.AddServices();
        }

        public void ConfigureApp(IApplicationBuilder app) { }
    }
}
