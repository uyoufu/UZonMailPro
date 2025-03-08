using System.Text.RegularExpressions;
using UZonMail.Core.PluginBase.Email;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.Utils.Email;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.EmailDecorators
{
    public partial class UnsubesribeButtonDecorator(SqlContext sqlContext, FunctionAccessService functionAccess) : IEmailBodyDecroator
    {
        public async Task<string> StartDecorating(IEmailDecoratorParams unsubesribeParams, string originBody)
        {
            if (string.IsNullOrEmpty(originBody)) return originBody;
            // 判断是否有企业版本功能
            if (!(await functionAccess.HasEmailTrackingAccess())) return originBody;
            var decoratorParams = unsubesribeParams as EmailDecoratorParams;

            var userReader = await CacheManager.Global.GetCache<UserInfoCache>(sqlContext, decoratorParams.SendingItem.UserId.ToString());
            var unsubscribeSettings = await CacheManager.Global.GetCache<UnsubscribeSettingsReader>(sqlContext, userReader.OrganizationId);

            // 说明没有设置 API 地址
            if (unsubscribeSettings == null || !unsubscribeSettings.EnableUnsubscribe) return originBody;
            if (string.IsNullOrEmpty(unsubscribeSettings.UnsubscribeUrl)) return originBody;

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
            var regex = _matchHref();
            var buttonResult = regex.Replace(butotnHtml, $"href=\"{unsubscribeUrl}\"");

            // 开始在最后添加一个退订按钮
            originBody += buttonResult;

            return originBody;
        }

        [GeneratedRegex("href=.*?\\s", RegexOptions.Multiline)]
        private static partial Regex _matchHref();
    }
}
