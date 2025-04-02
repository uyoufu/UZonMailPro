using log4net;
using System.Reflection;
using UZonMail.Utils.Http;

namespace UZonMailProPlugin.Modules.ByteDance.Signer
{
    /// <summary>
    /// 生成 tiktok Bogous
    /// xBogus
    /// </summary>
    public class TiktokCredentials : ByteDanceCredentials
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(TiktokCredentials));
        /// <summary>
        /// js 路径
        /// </summary>
        private readonly string _xBogusPath = string.Empty;

        public TiktokCredentials()
        {
            // 获取当前程序集路径
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            _xBogusPath = Path.Combine(assemblyDirectory, "Scripts/JS/x_bogus.js");
            //_logger.Info($"xBogusPath: {_xBogusPath}");
        }

        /// <summary>
        /// 获取 xbogus
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override string GetBogus(string query)
        {
            // 创建一个新的 Jint 引擎实例
            var engine = new JsEngine();
            // 执行 JavaScript 代码
            engine.ExecuteJsFile(_xBogusPath);
            // 调用 JavaScript 函数
            var result = engine.Invoke("sign", query, BrowserMock.GetChromeUserAgent());
            return result.ToString();
        }
    }
}
