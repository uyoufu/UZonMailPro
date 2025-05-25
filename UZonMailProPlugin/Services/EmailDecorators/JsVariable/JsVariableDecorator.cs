using Jint;
using log4net;
using System.Text.RegularExpressions;
using UZonMail.Core.Services.EmailDecorator;
using UZonMail.Core.Services.EmailDecorator.Interfaces;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.JsVariable;

namespace UZonMailProPlugin.Services.EmailDecorators.JsVariable
{
    public class JsVariableDecorator(SqlContext db, SqlContextPro dbPro, LicenseAccessService functionAccess) : IContentDecroator, IVariableResolver
    {
        private static ILog _logger = LogManager.GetLogger(typeof(JsVariableDecorator));

        // 延后执行
        public int Order => 1;

        /// <summary>
        /// 使用 js 进行变量替换
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="originContent"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> StartDecorating(IContentDecoratorParams decoratorParams, string originContent)
        {
            // 判断是否有企业版本功能
            if (!await functionAccess.HasJsVariableAccess()) return originContent;

            // 获取内容中的变量
            var variableNames = EmailVariableHelper.GetAllVariableNames(originContent);
            if (variableNames.Count == 0) return originContent;

            // 对变量进行替换
            var jsVariableCache = await CacheManager.Global.GetCache<JsVariableCache, SqlContextPro>(dbPro, decoratorParams.SendItemMeta.UserId);
            var uzonData = UzonData.GetUzonData(decoratorParams, jsVariableCache);

            var variableNameRegex = new Regex("/^[a-zA-Z_][a-zA-Z0-9_]*$/");

            foreach (var variableName in variableNames)
            {
                // 判断是否在函数变量中
                if (!jsVariableCache.Functions.TryGetValue(variableName, out var functionDefinition))
                {
                    var isVariableNameValid = variableNameRegex.IsMatch(variableName);
                    if (isVariableNameValid) continue;

                    // 可能是代码块，添加一个匿名函数
                    functionDefinition = new JsFunctionDefinition()
                    {
                        // 随机函数名
                        Name = "anonymous_" + Guid.NewGuid().ToString("N"),
                        FunctionBody = variableName
                    };
                }

                var jintEngin = new Engine().SetValue("uzonData", uzonData);
                jintEngin = jintEngin.Execute(functionDefinition.GetFunctionDefinition());


                // 开始测试
                try
                {
                    var jsValue = jintEngin.Invoke(functionDefinition.Name);
                    var serializer = new Jint.Native.Json.JsonSerializer(jintEngin);
                    string result = serializer.Serialize(jsValue).ToString();

                    // 进行替换
                    // 使用正则进行替换
                    var regex = new Regex(@"\{\{\s*" + Regex.Escape(variableName) + @"\s*\}\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    originContent = regex.Replace(originContent, result);
                }
                catch (Exception ex)
                {
                    _logger.Warn($"函数执行错误: {ex.Message}");
                }
            }

            return originContent;
        }
    }
}
