using Innofactor.EfCoreJsonValueConverter;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Base;

namespace UZonMailProPlugin.SQL.JsVariable
{
    /// <summary>
    /// 函数变量数据源
    /// </summary>
    public class JsVariableSource : UserAndOrgId
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
        [JsonField]
        public JToken? Value { get; set; }
    }
}
