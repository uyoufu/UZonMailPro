using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.ProPlugin.Services.License;
using UZonMail.ProPlugin.Services.Settings.Model;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.ReadingTracker;

namespace UZonMail.ProPlugin.Services.EmailBodyDecorators
{
    public class EmailTrackerDecoractor(
        SqlContext db,
        SqlContextPro dbPro,
        LicenseAccessService functionAccess,
        AppSettingsManager settingsManager
    ) : IContentDecroator
    {
        public int Order { get; }

        private static string? _baseUrl = string.Empty;
        private static readonly string _apiSettingKey = "baseApiUrl";

        public async Task<string> StartDecorating(
            IContentDecoratorParams trackerParams,
            string originBody
        )
        {
            var decoratorParams = trackerParams as EmailDecoratorParams;
            if (string.IsNullOrEmpty(originBody))
                return originBody;
            // 说明没有设置 API 地址
            if (_baseUrl == null)
                return originBody;

            // 判断是否有企业版本功能
            if (!(await functionAccess.HasEmailTrackingAccess()))
                return originBody;

            // 若已经存在追踪锚点则不再添加
            if (originBody.Contains("api/pro/email-tracker/image"))
                return originBody;
            // 判断是否设置了追踪
            var trackingSetting = await settingsManager.GetSetting<EmailTrackingSetting>(
                db,
                decoratorParams.SendingItem.UserId
            );
            if (!trackingSetting.IsEnableTracker())
                return originBody;

            // 添加跟踪锚点
            if (string.IsNullOrEmpty(_baseUrl))
            {
                // 获取 API 地址
                var systemSettings = await db.AppSettings.FirstOrDefaultAsync(x =>
                    x.Key == _apiSettingKey
                );
                if (systemSettings == null)
                {
                    _baseUrl = null;
                    return originBody;
                }
                _baseUrl = systemSettings.StringValue;
            }

            // 新建锚点
            var sendingItem = decoratorParams.SendingItem;
            var emailAnchor = await dbPro
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
                dbPro.EmailAnchors.Add(emailAnchor);
                await dbPro.SaveChangesAsync();
            }

            // 将链接添加到邮件中
            var imageHtml =
                $"<img src=\"{_baseUrl}/api/pro/email-tracker/image/{emailAnchor.ObjectId}\" style=\"display:none;\" />";
            originBody += imageHtml;

            return originBody;
        }
    }
}
