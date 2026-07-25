using System.Text.RegularExpressions;
using UzonMail.CorePlugin.Services.EmailDecorator.Interfaces;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;
using UzonMail.ProPlugin.Services.License;
using UzonMail.ProPlugin.Services.Settings.Model;
using UzonMail.ProPlugin.SQL;

namespace UzonMail.ProPlugin.Services.EmailBodyDecorators
{
    public partial class UnsubesribeButtonDecorator(
        SqlContext db,
        SqlContextPro dbPro,
        LicenseAccessService functionAccess,
        AppSettingsManager settingsManager,
        IDBCacheManager cacheManager
    ) : IContentDecroator
    {
        public int Order { get; }

        public async Task<string> StartDecorating(
            IContentDecoratorParams unsubesribeParams,
            string originBody
        )
        {
            if (string.IsNullOrEmpty(originBody))
                return originBody;
            // 判断是否有企业版本功能
            if (!(await functionAccess.HasEmailTrackingAccess()))
                return originBody;
            var userInfo = await cacheManager.GetCache<UserInfoCache>(
                db,
                unsubesribeParams.SendingItem.UserId
            );

            // 获取设置
            var unsubscribeSetting = await settingsManager.GetSetting<UnsubscribeSetting>(
                db,
                userInfo.UserId
            );
            await unsubscribeSetting.InitForSubscribling(dbPro, userInfo.OrganizationId);

            var unsubscribeUrl = await unsubscribeSetting.GetUnsubscribeUrl(db);

            // 说明没有设置 API 地址
            if (unsubscribeSetting == null || !unsubscribeSetting.IsEnable())
                return originBody;
            if (string.IsNullOrEmpty(unsubscribeUrl))
                return originBody;

            // 若已经存在追踪锚点则不再添加
            if (originBody.Contains(unsubscribeUrl))
                return originBody;

            // 生成退订链接
            unsubscribeUrl += $"&token={unsubesribeParams.SendingItem.ObjectId}";
            if (!unsubscribeUrl.Contains('?'))
            {
                unsubscribeUrl = unsubscribeUrl.Replace("&", "?");
            }

            var butotnHtml = unsubscribeSetting.UnsubscribeButtonHtml;
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
