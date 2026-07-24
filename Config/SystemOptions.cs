using System.Configuration;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Config
{
    public class SystemOptions
    {
        public string Name { get; set; } = "宇正群邮";
        public string LoginWelcome { get; set; } = "Welcome to UzonMail";
        public string Icon { get; set; }
        public string Copyright { get; set; } = "Copyright © since 2022 UZon Email";
        public string ICPInfo { get; set; } = "渝ICP备20246498号-3";

        public static SystemOptions DefaultSystemConfig()
        {
            return new SystemOptions();
        }

        public static SystemOptions GetSystemConfig(IConfiguration configuration)
        {
            var defaultConfig = DefaultSystemConfig();
            configuration.GetSection("System")?.Bind(defaultConfig);
            return defaultConfig;
        }
    }
}
