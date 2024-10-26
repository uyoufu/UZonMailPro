namespace UZonMailProPlugin.Services.EmailDecorators.UnsubscribeHeaders
{
    public class UnsubscribeHeaderFactory
    {
        private static Dictionary<string, IUnsubscribeHeader> _unsubscribeHeadersDic = new()
        {
            {"rfc8058",new RFC8058Header()}
        };

        /// <summary>
        /// 获取退订头部
        /// </summary>
        /// <param name="email">邮箱,如 abc@qq.com</param>
        /// <returns></returns>
        public static IUnsubscribeHeader GetUnsubscribeHeader(IServiceProvider serviceProvider, string email)
        {
            var unsubscribeConfig = new UnsubscribeSettings();
            // 获取配置
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            config.GetSection("Unsubscribe")?.Bind(unsubscribeConfig);
            var defaultHeader = _unsubscribeHeadersDic["rfc8058"];
            if (string.IsNullOrEmpty(email)) return defaultHeader;
            var type = email.Split('@').LastOrDefault();
            if (string.IsNullOrEmpty(type)) return defaultHeader;

            // 判断是否指定了退订头部
            var header = unsubscribeConfig.Headers.Find(x => x.Domain.Equals(type, StringComparison.CurrentCultureIgnoreCase));
            if (header != null) type = header.Domain.ToLower();

            if (_unsubscribeHeadersDic.TryGetValue(type, out IUnsubscribeHeader? value))
            {
                return value;
            }

            return defaultHeader;
        }
    }
}
