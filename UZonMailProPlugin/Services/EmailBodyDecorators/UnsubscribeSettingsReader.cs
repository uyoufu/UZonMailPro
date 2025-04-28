using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.Unsubscribes;

namespace UZonMailProPlugin.Services.EmailBodyDecorators
{
    public class UnsubscribeSettingsReader : BaseDBCache<SqlContextPro>
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UnsubscribeSettingsReader));

        #region 接口实现
        /// <summary>
        /// key 为 organizationId
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public override async Task Update(SqlContextPro db)
        {
            if (!NeedUpdate) return;
            SetDirty();

            // 开始添加
            var organizationId = LongValue;
            var unsubscribeSetting = await db.UnsubscribeSettings.AsNoTracking().FirstOrDefaultAsync(x => x.OrganizationId == organizationId);
            if (unsubscribeSetting == null)
            {
                _logger.Debug($"department {organizationId} has not set unsubscribe setting");
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

                _unsubscribeUrl = unsubscribeSetting.ExternalUrl;
                EnableUnsubscribe = true;
                return;
            }

            // 判断是否有退订页面
            var unsubscribePage = await db.UnsubscribePages.FirstOrDefaultAsync(x => x.OrganizationId == organizationId);
            if (unsubscribePage == null)
            {
                _logger.Error($"department {organizationId} UnsubscribePage is not set");
                EnableUnsubscribe = false;
                return;
            }


            EnableUnsubscribe = true;
        }

        public override void Dispose()
        {
            SetDirty(true);
        }
        #endregion

        #region 数据实现
        public bool EnableUnsubscribe { get; private set; }
        private string? _unsubscribeUrl;
        public string? UnsubscribeButtonHtml { get; private set; }

        /// <summary>
        /// 获取退订链接
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<string> GetUnsubscribeUrl(SqlContext db)
        {
            if (!EnableUnsubscribe) return string.Empty;

            if (!string.IsNullOrEmpty(_unsubscribeUrl)) return _unsubscribeUrl;

            // 获取 baseUrl
            var setting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == SystemSetting.BaseApiUrl);
            if (setting == null || string.IsNullOrEmpty(setting.StringValue) || !setting.StringValue.StartsWith("http"))
            {
                _logger.Warn("BaseApiUrl is not set");
                EnableUnsubscribe = false;
                return string.Empty;
            }
            _unsubscribeUrl = $"{setting.StringValue.TrimEnd('/')}/pages/unsubscribe/pls-give-me-a-shot";
            return _unsubscribeUrl;
        }
        #endregion
    }
}
