﻿using System.Text.RegularExpressions;
using UZonMail.Core.Services.Plugin;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.Utils.Email;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.Services.Settings.Model;
using UZonMailProPlugin.SQL;

namespace UZonMailProPlugin.Services.EmailBodyDecorators
{
    public partial class UnsubesribeButtonDecorator(SqlContext db, SqlContextPro dbPro, LicenseAccessService functionAccess,AppSettingsManager settingsManager) : IEmailBodyDecroator
    {
        public async Task<string> StartDecorating(IEmailDecoratorParams unsubesribeParams, string originBody)
        {
            if (string.IsNullOrEmpty(originBody)) return originBody;
            // 判断是否有企业版本功能
            if (!(await functionAccess.HasEmailTrackingAccess())) return originBody;
            var decoratorParams = unsubesribeParams as EmailDecoratorParams;

            var userInfo = await CacheManager.Global.GetCache<UserInfoCache>(db, decoratorParams.SendingItem.UserId.ToString());

            // 获取设置
            var unsubscribeSetting = await settingsManager.GetSetting<UnsubscribeSetting>(db, userInfo.UserId);
            await unsubscribeSetting.InitForSubscribling(dbPro, userInfo.OrganizationId);

            var unsubscribeUrl = await unsubscribeSetting.GetUnsubscribeUrl(db);

            // 说明没有设置 API 地址
            if (unsubscribeSetting == null || !unsubscribeSetting.IsEnable()) return originBody;
            if (string.IsNullOrEmpty(unsubscribeUrl)) return originBody;

            // 若已经存在追踪锚点则不再添加
            if (originBody.Contains(unsubscribeUrl)) return originBody;

            // 生成退订链接
            unsubscribeUrl += $"&token={decoratorParams.SendingItem.ObjectId}";
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
