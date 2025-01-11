namespace UZonMailProPlugin.Utils
{
    public class UserAgent
    {
        public static string GetBrowserName()
        {
            return "Mozilla";
        }

        public static string GetBrowserVersion()
        {
            return "5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0";
        }

        /// <summary>
        /// 获取 Win10 Chrome 浏览器的 UserAgent
        /// </summary>
        /// <returns></returns>
        public static string GetWin10ChromeUserAgent()
        {
            return $"{GetBrowserName()}/{GetBrowserVersion()}";
        }
    }
}
