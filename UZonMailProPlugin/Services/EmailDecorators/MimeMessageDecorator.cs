using MimeKit;
using UZonMail.DB.SQL;
using UZonMail.Utils.Email;
using UZonMailProPlugin.Services.EmailDecorators.UnsubscribeHeaders;
using UZonMail.DB.Managers.Cache;

namespace UZonMailProPlugin.Services.EmailDecorators
{
    public class MimeMessageDecorator : IMimeMessageDecroator
    {
        public async Task<MimeMessage> StartDecorating(EmailDecoratorParams decoratorParams, MimeMessage mimeMessage)
        {
            var db = decoratorParams.ServiceProvider.GetRequiredService<SqlContext>();

            var userInfo = await DBCacher.GetCache<UserInfoCache>(db, decoratorParams.SendingItem.UserId);
            var unsubscribeSettings = await DBCacher.GetCache<UnsubscribeSettingsReader>(db, userInfo.OrganizationId);

            // 没有启用退订
            if (unsubscribeSettings == null || !unsubscribeSettings.EnableUnsubscribe) return mimeMessage;

            // 生成退订链接
            var unsubscribeUrl = unsubscribeSettings.UnsubscribeUrl + $"&token={decoratorParams.SendingItem.ObjectId}";
            if (!unsubscribeUrl.Contains('?'))
            {
                unsubscribeUrl = unsubscribeUrl.Replace("&", "?");
            }
            var unsubscribeHeader = UnsubscribeHeaderFactory.GetUnsubscribeHeader(decoratorParams.ServiceProvider, decoratorParams.OutboxEmail);
            unsubscribeHeader.SetHeader(mimeMessage, unsubscribeUrl);

            return mimeMessage;
        }
    }
}
