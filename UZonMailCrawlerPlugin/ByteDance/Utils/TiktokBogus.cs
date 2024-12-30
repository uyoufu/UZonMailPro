using Jint;
using System.Reflection;

namespace UZonMailCrawlerPlugin.ByteDance.Utils
{
    /// <summary>
    /// 生成 tiktok Bogous
    /// xBogus
    /// </summary>
    public class TiktokBogus : ByteDanceBogus
    {
        /// <summary>
        /// js 路径
        /// </summary>
        private readonly string _xBogusPath = string.Empty;

        public TiktokBogus()
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
            var result = engine.Invoke("sign", requestUrl);
            return result.ToString();
        }
    }
}
