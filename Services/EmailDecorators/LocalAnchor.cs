using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UzonMail.DB.SQL;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.ReadingTracker;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Services.EmailBodyDecorators
{
    public class LocalAnchor(SqlContextPro sqlContext) : ITransientService
    {
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
                    RecipientEmails = string.Join(",", sendingItem.Recipients.Select(x => x.Email)),
                    SenderEmail = decoratorParams.SenderEmail
                };
                sqlContext.EmailAnchors.Add(emailAnchor);
                await sqlContext.SaveChangesAsync();

                // 向远程推送邮件锚点记录
            }

            return emailAnchor;
        }
    }
}
