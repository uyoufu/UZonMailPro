using UZonMail.CorePlugin.Database.Updater;
using UZonMail.DB.SQL;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.Unsubscribes;

namespace UZonMail.ProPlugin.Database.Updaters
{
    /// <summary>
    /// 系统默认调用
    /// </summary>
    public class InitUnsubscribe(SqlContextPro db) : IDatabaseUpdater
    {
        public Version Version => new("0.1.1.0");

        public async Task Update()
        {
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
            db.UnsubscribeButtons.Add(unsubscribeButton);

            // 增加退订页面
            var unsubscribePage = new UnsubscribePage()
            {
                // admin 默认组织
                OrganizationId = 3,
                Language = "en-US",
                HtmlContent =
                    "<div class=\"column items-center full-height\">\r\n<h5>Unsubscribe from Emails</h5>\r\n<p>If you no longer wish to receive emails from us, please click \"Unsubscribe\".</p>\r\n</div>"
            };
            db.UnsubscribePages.Add(unsubscribePage);
            await db.SaveChangesAsync();
        }
    }
}
