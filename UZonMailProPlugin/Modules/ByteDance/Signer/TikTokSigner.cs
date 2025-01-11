using Jint;
using System.Reflection;
using UZonMailProPlugin.Utils;

namespace UZonMailProPlugin.Modules.ByteDance.Signer
{
    /// <summary>
    /// 生成 tiktok Bogous
    /// xBogus
    /// </summary>
    public class TiktokSigner : ByteDanceSigner
    {
        /// <summary>
        /// js 路径
        /// </summary>
        private readonly string _xBogusPath = string.Empty;

        public TiktokSigner()
        {
            // 获取当前程序集路径
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            _xBogusPath = Path.Combine(assemblyDirectory, "ByteDance/JS/x_bogus.js");
        }

        /// <summary>
        /// 获取 xbogus
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public override string GetBogus(string requestUrl)
        {
            // 创建一个新的 Jint 引擎实例
            var engine = new JsEngine();
            // 执行 JavaScript 代码
            engine.ExecuteJsFile(_xBogusPath);
            // 调用 JavaScript 函数
            var result = engine.Invoke("sign", requestUrl, UserAgent.GetWin10ChromeUserAgent());
            return result.ToString();
        }

        /// <summary>
        /// 获取登陆信息
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public override SignResult Sign(string requestUrl)
        {
            var msToken = GetMsToken();
            var signature = GetSignature();
            var result = new TikTokSignResult(requestUrl, msToken, signature);
            var fullRequestUrl = result.GetMsTokenUrl();
            result.Bogus = GetBogus(fullRequestUrl);
            return result;
        }
    }
}
