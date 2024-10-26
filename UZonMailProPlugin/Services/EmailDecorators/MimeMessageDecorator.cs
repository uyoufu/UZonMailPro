using MimeKit;
using System.Text.RegularExpressions;
using UZonMail.DB.SQL.Organization;
using UZonMail.DB.SQL;
using UZonMail.Managers.Cache;
using UZonMail.Utils.Email;
using UZonMailProPlugin.Services.EmailDecorators.UnsubscribeHeaders;

namespace UZonMailProPlugin.Services.EmailDecorators
{
    public class MimeMessageDecorator : IMimeMessageDecroator
    {
        public async Task<MimeMessage> StartDecorating(EmailDecoratorParams decoratorParams, MimeMessage mimeMessage)
        {
            var db = decoratorParams.ServiceProvider.GetRequiredService<SqlContext>();

            var userReader = await CacheManager.GetCache<UserReader>(db, decoratorParams.SendingItem.UserId.ToString());
            var unsubscribeSettings = await CacheManager.GetCache<UnsubscribeSettingsReader>(db, userReader.OrganizationObjectId);

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
