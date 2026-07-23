using System.Linq;
using Newtonsoft.Json.Linq;
using UZonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.ProPlugin.Services.EmailDecorators.JsVariable
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
        public string OutboxEmail { get; set; }

        /// <summary>
        /// 收件箱
        /// </summary>
        public string InboxEmail { get; set; }

        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 当前日期
        /// </summary>
        public DateTime DateNow { get; set; } = DateTime.UtcNow;

        public EmailAddress Outbox { get; set; }

        public EmailAddress Inbox { get; set; }

        /// <summary>
        /// 收件箱
        /// </summary>
        public List<EmailAddress> Inboxes { get; set; } = [];

        /// <summary>
        /// 抄送人
        /// </summary>
        public List<EmailAddress> CC { get; set; } = [];

        /// <summary>
        /// 密送
        /// </summary>
        public List<EmailAddress> BCC { get; set; } = [];

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
                OutboxEmail = "out@test.com",
                InboxEmail = "in@test.com",
                Body = "This is a test email body.",
                Outbox = new EmailAddress() { Email = "out@test.com", Name = "outbox" },
                Inbox = new EmailAddress() { Email = "in@test.com", Name = "inbox" },
                Inboxes = [new EmailAddress() { Email = "in@test.com", Name = "inbox" }],
                CC = [],
                BCC = [],
            };

            return uzonData;
        }

        /// <summary>
        /// 获取 uzonData 数据
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <param name="variableCache"></param>
        /// <returns></returns>
        public static UzonData GetUzonData(
            IContentDecoratorParams decoratorParams,
            JsVariableCache variableCache
        )
        {
            var sendItemMeta = decoratorParams.SendItemMeta;

            var uzonData = new UzonData()
            {
                Source = variableCache.Source,
                Data = sendItemMeta.BodyData ?? new JObject(),
                Subject = sendItemMeta.Subject,
                OutboxEmail = decoratorParams.Outbox.Email,
                InboxEmail = string.Join(",", sendItemMeta.Inboxes.Select(x => x.Email)),
                Body = sendItemMeta.HtmlBody,

                // 完整数据
                Outbox = new EmailAddress()
                {
                    Email = decoratorParams.Outbox.Email,
                    Name = decoratorParams.Outbox.Name
                },
                Inbox = sendItemMeta.Inboxes.First(),
                Inboxes = sendItemMeta.Inboxes,
                CC = sendItemMeta.CC ?? [],
                BCC = sendItemMeta.BCC ?? [],
            };

            return uzonData;
        }
    }
}
