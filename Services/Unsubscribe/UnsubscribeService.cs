using log4net;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.EmailReceiving.Application;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.Unsubscribes;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Services.Unsubscribe
{
    public class UnsubscribeService(
        SqlContext db,
        SqlContextPro dbPro,
        IRecipientSuppressionService recipientSuppressionService
    ) : IScopedService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UnsubscribeService));

        /// <summary>
        /// 开始退订
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string sendingItemId, string? host)
        {
            var sendingItem = await db
                .SendingItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ObjectId == sendingItemId);
            if (sendingItem == null)
            {
                var message = "无法解析退订 token";
                _logger.Error(message);
                throw new KnownException(message);
            }
            var user = await db
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sendingItem.UserId);
            if (user is null)
                throw new KnownException("退订邮件所属用户不存在");

            var toEmails = (sendingItem.ToEmails ?? string.Empty).Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            foreach (var toEmail in toEmails)
            {
                // 添加到退订列表
                var existOne = await dbPro.UnsubscribeEmails.FirstOrDefaultAsync(x =>
                    x.OrganizationId == user.OrganizationId && x.Email == toEmail
                );
                if (existOne != null)
                {
                    continue;
                }

                // 新增退订
                var unsubscribeEmail = new UnsubscribeEmail()
                {
                    Email = toEmail,
                    OrganizationId = user.OrganizationId,
                    Host = host
                };
                dbPro.UnsubscribeEmails.Add(unsubscribeEmail);
            }
            await dbPro.SaveChangesAsync();
            foreach (var toEmail in toEmails)
            {
                await recipientSuppressionService.SuppressAsync(
                    user.OrganizationId,
                    toEmail,
                    RecipientSuppressionReason.Unsubscribe,
                    sendingItem.UserId,
                    "Professional unsubscribe callback"
                );
            }
            return true;
        }

        /// <summary>
        /// 是否已经退订
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <returns></returns>
        public async Task<bool> IsUnsubscribed(string sendingItemId)
        {
            var sendingItem = await db
                .SendingItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ObjectId == sendingItemId);
            if (sendingItem == null)
            {
                var message = "无法解析退订 token";
                _logger.Error(message);
                throw new KnownException(message);
            }
            var user = await db
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sendingItem.UserId);
            if (user is null)
                throw new KnownException("退订邮件所属用户不存在");

            var toEmails = (sendingItem.ToEmails ?? string.Empty).Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            if (toEmails.Length != 1)
                return false;

            var suppressedEmails = await recipientSuppressionService.GetSuppressedEmailsAsync(
                user.OrganizationId,
                toEmails
            );
            return suppressedEmails.Count > 0;
        }
    }
}
