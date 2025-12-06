using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.SendCore.Interfaces;
using UZonMail.DB.Extensions;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.ProPlugin.SQL;

namespace UZonMail.ProPlugin.Services.Unsubscribe
{
    public class UnsubscribeItemFilter(SqlContext sqlContext, SqlContextPro dbPro)
        : ISendingItemFilter
    {
        public async Task<List<long>> GetInvalidSendingItemIds(List<SendingItem> sendingItems)
        {
            if (sendingItems == null || sendingItems.Count == 0)
            {
                return [];
            }

            // 对于取消订阅的邮件，进行标记
            var userInfo = await DBCacheManager.Global.GetCache<UserInfoCache>(
                sqlContext,
                sendingItems[0].UserId
            );
            var unsubscribedEmails = await GetUnsubscribedEmails(userInfo.OrganizationId);
            if (unsubscribedEmails.Count == 0)
                return [];

            var unsubscribedSendingItemIds = sendingItems
                .Where(x => x.Inboxes.Any(i => unsubscribedEmails.Contains(i.Email)))
                .Select(x => x.Id)
                .ToList();
            if (unsubscribedSendingItemIds.Count > 0)
            {
                // 更新邮件状态
                await sqlContext.SendingItems.UpdateAsync(
                    x => unsubscribedSendingItemIds.Contains(x.Id),
                    x =>
                        x.SetProperty(y => y.Status, SendingItemStatus.Unsubscribed)
                            .SetProperty(y => y.SendDate, DateTime.UtcNow)
                            .SetProperty(y => y.SendResult, "收件人已取消订阅")
                );
            }

            return unsubscribedSendingItemIds;
        }

        /// <summary>
        /// 获取组织中所有退订的邮箱
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        private async Task<List<string>> GetUnsubscribedEmails(long organizationId)
        {
            var unsubscribedEmails = await dbPro
                .UnsubscribeEmails.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId)
                .Where(x => !x.IsDeleted)
                .Select(x => x.Email)
                .ToListAsync();

            return unsubscribedEmails;
        }
    }
}
