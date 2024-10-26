using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Settings;
using UZonMail.DB.SQL.Unsubscribes;
using UZonMail.Managers.Cache;

namespace UZonMailProPlugin.Services.EmailDecorators
{
    public class UnsubscribeSettingsReader : ICacheReader
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UnsubscribeSettingsReader));

        #region 接口实现
        public string SettingKey { get; private set; }

        private bool _needUpdate = true;

        /// <summary>
        /// key 为 userId
        /// </summary>
        /// <param name="db"></param>
        /// <param name="organizationObjectId"></param>
        /// <returns></returns>
        public async Task Initialize(SqlContext db, string organizationObjectId)
        {
            if (!_needUpdate) return;
            _needUpdate = false;

            // 开始添加
            SettingKey = CacheManager.GetFullKey<UnsubscribeSettingsReader>(organizationObjectId);

            var organization = await db.Departments.FirstOrDefaultAsync(x => x.ObjectId == organizationObjectId);
            var unsubscribeSetting = await db.UnsubscribeSettings.AsNoTracking().FirstOrDefaultAsync(x => x.OrganizationId == organization.Id);
            if (unsubscribeSetting == null)
            {
                _logger.Debug($"{organization.Name} has not set unsubscribe setting");
                EnableUnsubscribe = false;
                return;
            }

            if (!unsubscribeSetting.Enable)
            {
                EnableUnsubscribe = false;
                return;
            }

            // 获取退订按钮
            var unsubscribeButton = await db.UnsubscribeButtons.FirstOrDefaultAsync();
            if (unsubscribeButton == null)
            {
                _logger.Error("UnsubscribeButton is not set");
                EnableUnsubscribe = false;
                return;
            }
            UnsubscribeButtonHtml = unsubscribeButton.ButtonHtml;

            // 生成退订链接
            if (unsubscribeSetting.Type == UnsubscibeType.External)
            {
                // 判断是否有外部链接
                if (string.IsNullOrEmpty(unsubscribeSetting.ExternalUrl) || !unsubscribeSetting.ExternalUrl.StartsWith("http"))
                {
                    _logger.Error("ExternalUrl is not set");
                    EnableUnsubscribe = false;
                    return;
                }

                UnsubscribeUrl = unsubscribeSetting.ExternalUrl;
                EnableUnsubscribe = true;
                return;
            }

            // 判断是否有退订页面
            var unsubscribePage = await db.UnsubscribePages.FirstOrDefaultAsync(x => x.OrganizationId == organization.Id);
            if(unsubscribePage == null)
            {
                _logger.Error("UnsubscribePage is not set");
                EnableUnsubscribe = false;
                return;
            }

            // 获取 baseUrl
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == SystemSetting.BaseApiUrl);
            if (setting == null || string.IsNullOrEmpty(setting.StringValue) || !setting.StringValue.StartsWith("http"))
            {
                _logger.Warn("BaseApiUrl is not set");
                EnableUnsubscribe = false;
                return;
            }
            UnsubscribeUrl = $"{setting.StringValue.TrimEnd('/')}/pages/unsubscribe/pls-give-me-a-shot";
            EnableUnsubscribe = true;
        }

        public void NeedUpdate()
        {
            _needUpdate = false;
        }
        #endregion

        #region 数据实现
        public bool EnableUnsubscribe { get; private set; }
        public string? UnsubscribeUrl { get; private set; }
        public string? UnsubscribeButtonHtml { get; private set; }
        #endregion
    }
}
