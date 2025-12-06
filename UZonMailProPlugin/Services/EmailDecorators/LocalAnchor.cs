using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.ReadingTracker;

namespace UZonMail.ProPlugin.Services.EmailBodyDecorators
{
    public class LocalAnchor(SqlContextPro sqlContext, HttpClient httpClient) : ITransientService
    {
        private static readonly string _anchorUrl = "api/pro/email-tracker/image";

        /// <summary>
        /// 获取邮件锚点
        /// </summary>
        /// <param name="decoratorParams"></param>
        /// <returns></returns>
        public async Task<EmailAnchor> GetEmailAnchor(EmailDecoratorParams decoratorParams)
        {
            var sendingItem = decoratorParams.SendingItem;
            var emailAnchor = await sqlContext
                .EmailAnchors.Where(x => x.SendingItemId == sendingItem.Id)
                .FirstOrDefaultAsync();
            if (emailAnchor == null)
            {
                // 新增
                emailAnchor = new EmailAnchor
                {
                    UserId = sendingItem.UserId,
                    SendingItemId = sendingItem.Id,
                    SendingGroupId = sendingItem.SendingGroupId,
                    InboxEmails = string.Join(",", sendingItem.Inboxes.Select(x => x.Email)),
                    OutboxEmail = decoratorParams.OutboxEmail
                };
                sqlContext.EmailAnchors.Add(emailAnchor);
                await sqlContext.SaveChangesAsync();

                // 向远程推送邮件锚点记录
            }

            return emailAnchor;
        }
    }
}
