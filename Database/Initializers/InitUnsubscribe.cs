using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Database.Initializers;
using UzonMail.CorePlugin.Database.Upgrade;
using UzonMail.CorePlugin.Services.EmailReceiving.Application;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.EmailReceiving;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.Unsubscribes;

namespace UzonMail.ProPlugin.Database.Initializers
{
    /// <summary>
    /// 系统默认调用
    /// </summary>
    public class InitUnsubscribe(
        SqlContext db,
        SqlContextPro dbPro,
        IRecipientSuppressionService recipientSuppressionService
    ) : IDbInitializer
    {
        public string Name => nameof(InitUnsubscribe);

        public async Task ExecuteAsync()
        {
            await MigrateRecipientSuppressionsAsync();
            if (dbPro.UnsubscribePages.Any())
                return;

            // 增加退订相关的表
            var unsubscribeButton = new UnsubscribeButton()
            {
                // admin 的默认组织
                OrganizationId = 3,
                Name = "Unsubscribe",
                Description = "Unsubscribe button",
                ButtonHtml =
                    "<div style=\"display: flex; flex-direction: column; align-items: center;\">\r\n  <a href=\"unsubscribe/pls-give-me-a-shot\" class=\"button\" title=\"Click to unsubscribe\" target=\"_blank\"\r\n    style=\"display: inline-block; color: #7367f0; border: none; text-align: center; text-decoration: underline; cursor: pointer; padding: 2px 4px; border-radius: 3px; font-size: 0.7rem;\">\r\n    UNSUBSCRIBE\r\n  </a>\r\n</div>"
            };
            dbPro.UnsubscribeButtons.Add(unsubscribeButton);

            // 增加退订页面
            var unsubscribePage = new UnsubscribePage()
            {
                // admin 默认组织
                OrganizationId = 3,
                Language = "en-US",
                HtmlContent =
                    "<div class=\"column items-center full-height\">\r\n<h5>Unsubscribe from Emails</h5>\r\n<p>If you no longer wish to receive emails from us, please click \"Unsubscribe\".</p>\r\n</div>"
            };
            dbPro.UnsubscribePages.Add(unsubscribePage);
            await dbPro.SaveChangesAsync();
        }

        private async Task MigrateRecipientSuppressionsAsync()
        {
            var unsubscribeEmails = await dbPro
                .UnsubscribeEmails.AsNoTracking()
                .Select(unsubscribe => new { unsubscribe.OrganizationId, unsubscribe.Email })
                .Distinct()
                .ToListAsync();
            if (unsubscribeEmails.Count == 0)
                return;
            var organizationIds = unsubscribeEmails
                .Select(unsubscribe => unsubscribe.OrganizationId)
                .Distinct()
                .ToArray();
            var actorUserIds = await db
                .Users.AsNoTracking()
                .Where(user => organizationIds.Contains(user.OrganizationId))
                .GroupBy(user => user.OrganizationId)
                .ToDictionaryAsync(group => group.Key, group => group.Min(user => user.Id));
            foreach (var unsubscribeEmail in unsubscribeEmails)
            {
                if (!actorUserIds.TryGetValue(unsubscribeEmail.OrganizationId, out var actorUserId))
                    continue;
                await recipientSuppressionService.SuppressAsync(
                    unsubscribeEmail.OrganizationId,
                    unsubscribeEmail.Email,
                    RecipientSuppressionReason.Unsubscribe,
                    actorUserId,
                    "Migrated from Professional unsubscribe list"
                );
            }
        }
    }
}
