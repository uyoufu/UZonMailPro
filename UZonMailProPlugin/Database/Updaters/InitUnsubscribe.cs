using UZonMail.Core.Database.Updater;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Unsubscribes;

namespace UZonMailProPlugin.Database.Updaters
{
    /// <summary>
    /// 系统默认调用
    /// </summary>
    public class InitUnsubscribe(SqlContext db) : IDatabaseUpdater
    {
        public Version Version => new("0.1.1.0");

        public async Task Update()
        {
            // 增加退订相关的表
            var unsubscribeButton = new UnsubscribeButton()
            {
                Name = "Unsubscribe",
                Description = "Unsubscribe button",
                ButtonHtml = @"<div style=""display: flex;flex-direction: column;align-items: center;"">
                                 <a href=""unsubscribe/pls-give-me-a-shot"" class=""button"" title=""Click to unsubscribe"" target=""_blank""
                                   style=""display: inline-block; color: #7367f0; border: none; text-align: center; text-decoration: underline; cursor: pointer;"">Unsubscribe</a>
                               </div>"
            };
            db.Add(unsubscribeButton);

            // 增加退订页面
            var unsubscribePage = new UnsubscribePage()
            {
                // admin 默认组织
                OrganizationId = 3,
                Language = "en-US",
                HtmlContent = "<div class=\"column items-center full-height\">\r\n<h5>Unsubscribe from Emails</h5>\r\n<p>If you no longer wish to receive emails from us, please click \"Unsubscribe\".</p>\r\n</div>"
            };
            db.Add(unsubscribePage);

            var unsubscribeSettings = new UnsubscribeSetting()
            {
                Enable = false,
                OrganizationId = 3,
                UnsubscribeButtonId = 1
            };
            db.Add(unsubscribeSettings);
            await db.SaveChangesAsync();
        }
    }
}
