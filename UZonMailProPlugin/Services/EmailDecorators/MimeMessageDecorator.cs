using MimeKit;
using UZonMail.DB.SQL;
using UZonMail.Utils.Email;
using UZonMailProPlugin.Services.EmailDecorators.UnsubscribeHeaders;
using UZonMail.DB.Managers.Cache;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.EmailDecorators
{
    /// <summary>
    /// 在消息体中添加退订链接
    /// </summary>
    public class MimeMessageDecorator(IServiceProvider serviceProvider,SqlContext sqlContext, FunctionAccessService functionAccess) : IMimeMessageDecroator
    {
        public async Task<MimeMessage> StartDecorating(EmailDecoratorParams decoratorParams, MimeMessage mimeMessage)
        {
            if (!(await functionAccess.HasUnsubscribeAccess())) return mimeMessage;

            var userInfo = await DBCacher.GetCache<UserInfoCache>(sqlContext, decoratorParams.SendingItem.UserId);
            var unsubscribeSettings = await DBCacher.GetCache<UnsubscribeSettingsReader>(sqlContext, userInfo.OrganizationId);

            // 没有启用退订
            if (unsubscribeSettings == null || !unsubscribeSettings.EnableUnsubscribe) return mimeMessage;

            // 生成退订链接
            var unsubscribeUrl = unsubscribeSettings.UnsubscribeUrl + $"&token={decoratorParams.SendingItem.ObjectId}";
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
