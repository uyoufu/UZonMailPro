using UzonMail.DB.SQL.Base;

namespace UzonMail.ProPlugin.SQL.Unsubscribes
{
    /// <summary>
    /// 取消订阅的界面设置
    /// </summary>
    public class UnsubscribePage : OrgId
    {
        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Html 内容
        /// </summary>
        public string HtmlContent { get; set; } = string.Empty;

        /// <summary>
        /// 是否是默认的退订页面
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
