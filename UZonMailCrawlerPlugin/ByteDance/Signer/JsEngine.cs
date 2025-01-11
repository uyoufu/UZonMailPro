using Jint;
using Jint.Native;

namespace UZonMailCrawlerPlugin.ByteDance.Utils
{
    public class JsEngine
    {
        private static readonly Lazy<Engine> _jsEngine = new(() => new Engine());
        private static readonly Dictionary<string, bool> _executedFiles = [];

        /// <summary>
        /// 执行 js 代码文件
        /// </summary>
        /// <param name="jsPath"></param>
        /// <returns></returns>
        public JsEngine ExecuteJsFile(string jsPath)
        {
            if (!File.Exists(jsPath)) throw new Exception("js 文件不存在");
            if (_executedFiles.ContainsKey(jsPath)) return this;

            var jsCode = File.ReadAllText(jsPath);
            _jsEngine.Value.Execute(jsCode);
            _executedFiles.Add(jsPath, true);

            return this;
        }

        /// <summary>
        /// 执行 js 代码
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public JsValue Invoke(string propertyName, params object[] args)
        {
            var engine = _jsEngine.Value;
            return engine.Invoke(propertyName, args);
        }
    }
}
