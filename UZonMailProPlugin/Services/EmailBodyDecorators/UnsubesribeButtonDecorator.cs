using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Uamazing.Utils.Email;
using UZonMail.DB.MySql;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Organization;
using UZonMail.DB.SQL.Settings;
using UZonMail.Managers.Cache;

namespace UZonMailProPlugin.Services.EmailBodyDecorators
{
    public class UnsubesribeButtonDecorator : IEmailBodyDecroator
    {
        public async Task<string> StartDecorating(EmailBodyDecoratorParams decoratorParams, string originBody)
        {
            if (string.IsNullOrEmpty(originBody)) return originBody;
            var db = decoratorParams.ServiceProvider.GetRequiredService<SqlContext>();

            var userReader = await CacheManager.GetCache<UserReader>(db, decoratorParams.SendingItem.UserId.ToString());
            var unsubscribeSettings = await CacheManager.GetCache<UnsubscribeSettingsReader>(db, userReader.OrganizationObjectId);

            // 说明没有设置 API 地址
            if (unsubscribeSettings == null || !unsubscribeSettings.EnableUnsubscribe) return originBody;
            // 若已经存在追踪锚点则不再添加
            if (originBody.Contains(unsubscribeSettings.UnsubscribeUrl)) return originBody;

            // 生成退订链接
            var unsubscribeUrl = unsubscribeSettings.UnsubscribeUrl + $"&token={decoratorParams.SendingItem.ObjectId}";
            if (!unsubscribeUrl.Contains('?'))
            {
                unsubscribeUrl = unsubscribeUrl.Replace("&", "?");
            }

            var butotnHtml = unsubscribeSettings.UnsubscribeButtonHtml;
            // 将 src="" 替换为退订链接
            var regex = new Regex("href=.*?\\s",RegexOptions.Multiline);
            var buttonResult = regex.Replace(butotnHtml, $"href=\"{unsubscribeUrl}\"");

            // 开始在最后添加一个退订按钮
            originBody += buttonResult;

            return originBody;
        }
    }
}
