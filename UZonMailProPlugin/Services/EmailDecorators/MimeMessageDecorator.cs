using MimeKit;
using UZonMail.Core.Services.EmailDecorator.Interfaces;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMailProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.Services.Settings.Model;
using UZonMailProPlugin.SQL;

namespace UZonMailProPlugin.Services.EmailBodyDecorators
{
    /// <summary>
    /// 在消息体中添加退订链接
    /// </summary>
    public class MimeMessageDecorator(IServiceProvider serviceProvider, SqlContext db, SqlContextPro dbPro, LicenseAccessService functionAccess)
        : IMimeMessageDecroator
    {
        public async Task<MimeMessage> StartDecorating(IContentDecoratorParams mimeParams, MimeMessage mimeMessage)
        {
            if (!(await functionAccess.HasUnsubscribeAccess())) return mimeMessage;

            var decoratorParams = mimeParams as EmailDecoratorParams;
            var userInfo = await CacheManager.Global.GetCache<UserInfoCache>(db, decoratorParams.SendingItem.UserId);

            var unsubscribeSettings = await serviceProvider.GetRequiredService<AppSettingsManager>().GetSetting<UnsubscribeSetting>(db,userInfo.UserId);
            await unsubscribeSettings.InitForSubscribling(dbPro, userInfo.OrganizationId);

            // 没有启用退订
            if (unsubscribeSettings == null || !unsubscribeSettings.IsEnable()) return mimeMessage;

            // 生成退订链接
            var unsubscribeUrl = await unsubscribeSettings.GetUnsubscribeUrl(db);
            unsubscribeUrl += $"&token={decoratorParams.SendingItem.ObjectId}";

            if (!unsubscribeUrl.Contains('?'))
            {
                unsubscribeUrl = unsubscribeUrl.Replace("&", "?");
            }
            var unsubscribeHeader = UnsubscribeHeaderFactory.GetUnsubscribeHeader(serviceProvider, decoratorParams.OutboxEmail);
            unsubscribeHeader.SetHeader(mimeMessage, unsubscribeUrl);

            return mimeMessage;
        }
    }
}
