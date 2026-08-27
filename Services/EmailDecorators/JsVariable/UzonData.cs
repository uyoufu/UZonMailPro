using System.Linq;
using Newtonsoft.Json.Linq;
using UzonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.ProPlugin.Services.EmailDecorators.JsVariable
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
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// 发件箱
        /// </summary>
        public string SenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// 收件箱
        /// </summary>
        public string RecipientEmail { get; set; } = string.Empty;

        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// 当前日期
        /// </summary>
        public DateTime DateNow { get; set; } = DateTime.UtcNow;

        public EmailAddress SenderAccount { get; set; } = new();

        public EmailAddress RecipientContact { get; set; } = new();

        /// <summary>
        /// 收件箱
        /// </summary>
        public List<EmailAddress> RecipientContacts { get; set; } = [];

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
                SenderEmail = "out@test.com",
                RecipientEmail = "in@test.com",
                Body = "This is a test email body.",
                SenderAccount = new EmailAddress()
                {
                    Email = "out@test.com",
                    Name = "senderAccount"
                },
                RecipientContact = new EmailAddress()
                {
                    Email = "in@test.com",
                    Name = "recipientContact"
                },
                RecipientContacts =
                [
                    new EmailAddress() { Email = "in@test.com", Name = "recipientContact" }
                ],
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
            var uzonData = new UzonData()
            {
                Source = variableCache.Source,
                Data = decoratorParams.Variables ?? new JObject(),
                Subject = decoratorParams.Subject,
                SenderEmail = decoratorParams.SenderAccount.Email,
                RecipientEmail = string.Join(",", decoratorParams.Recipients.Select(x => x.Email)),
                Body = decoratorParams.HtmlBody,

                // 完整数据
                SenderAccount = new EmailAddress()
                {
                    Email = decoratorParams.SenderAccount.Email,
                    Name = decoratorParams.SenderAccount.Name
                },
                RecipientContact = decoratorParams.Recipients.First(),
                RecipientContacts = [.. decoratorParams.Recipients],
                CC = [.. decoratorParams.CC],
                BCC = [.. decoratorParams.BCC],
            };

            return uzonData;
        }
    }
}
