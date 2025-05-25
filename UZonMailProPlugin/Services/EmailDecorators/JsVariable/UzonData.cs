using Newtonsoft.Json.Linq;
using System.Linq;
using UZonMail.Core.Services.EmailDecorator.Interfaces;

namespace UZonMailProPlugin.Services.EmailDecorators.JsVariable
{
    /// <summary>
    /// uzon 数据定义类
    /// </summary>
    public class UzonData
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public JObject Source { get; set; } = [];

        /// <summary>
        /// Excel 数据
        /// </summary>
        public JObject Data { get; set; } = [];

        /// <summary>
        /// 主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 发件箱
        /// </summary>
        public string Outbox { get; set; }

        /// <summary>
        /// 收件箱
        /// </summary>
        public string Inbox { get; set; }

        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 当前日期
        /// </summary>
        public DateTime DateNow { get; set; } = DateTime.Now;

        /// <summary>
        /// 获取测试数据
        /// </summary>
        /// <param name="variableCache"></param>
        /// <returns></returns>
        public static UzonData GetTestUzonData(JsVariableCache variableCache)
        {
            var uzonData = new UzonData()
            {
                Source = variableCache.Source,
                Subject = "Default Subject",
                Outbox = "out@test.com",
                Inbox = "in@test.com",
                Body = "This is a test email body.",
            };

            return uzonData;
        }

        /// <summary>
        /// 获取 uzonData 数据
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="variableCache"></param>
        /// <returns></returns>
        public static UzonData GetUzonData(IContentDecoratorParams decoratorParams, JsVariableCache variableCache)
        {
            var sendItemMeta = decoratorParams.SendItemMeta;

            var uzonData = new UzonData()
            {
                Source = variableCache.Source,
                Data = sendItemMeta.BodyData ?? new JObject(),
                Subject = sendItemMeta.Subject,
                Outbox = decoratorParams.OutboxEmail,
                Inbox = string.Join(",", sendItemMeta.Inboxes.Select(x => x.Email)),
                Body = sendItemMeta.HtmlBody,
            };

            return uzonData;
        }
    }
}
