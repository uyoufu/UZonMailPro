using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Base;

namespace UZonMail.ProPlugin.SQL.JsVariable
{
    /// <summary>
    /// JavaScript 函数定义
    /// </summary>
    [Index(nameof(UserId), nameof(Name), IsUnique = true)]
    public class JsFunctionDefinition : UserAndOrgId
    {
        /// <summary>
        /// 函数名称
        /// 不能重复
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 函数描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 函数体
        /// </summary>
        public string FunctionBody { get; set; } = string.Empty;

        /// <summary>
        /// 获取函数定义
        /// 1. 若是匿名函数，返回函数名为 Name 的函数定义
        /// 2. 若是非匿名函数，将函数名改为 Name，并返回函数定义
        /// 3. 若只包含函数体，返回一个 Name 函数
        /// </summary>
        /// <returns></returns>
        public string GetFunctionDefinition()
        {            
            // 去掉 function ...) 部分
            var regex = new Regex(".*function.*\\)", RegexOptions.IgnoreCase);
            var body = regex.Replace(FunctionBody, "");
            // 去掉首尾的 { 和 } 号
            body = body.Trim().TrimStart('{').TrimEnd('}').Trim();

            // 若只有一行，则自动添加 return
            if (!body.Contains('\n') && !body.Contains("return"))
            {
                body = $"return {body};";
            }

            // 包装为 function Name
            return $"function {Name}() {{\n{body}\n}}";
        }
    }
}
