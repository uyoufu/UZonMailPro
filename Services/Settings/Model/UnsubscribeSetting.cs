using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.Settings.Model;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.ProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.Unsubscribes;

namespace UZonMail.ProPlugin.Services.Settings.Model
{
    public enum UnsubscibeType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 系统
        /// </summary>
        System,

        /// <summary>
        /// 外部链接
        /// </summary>
        External
    }

    public class UnsubscribeSetting : BaseSettingModel
    {
        private static ILog _logger = LogManager.GetLogger(typeof(UnsubscribeSetting));

        /// <summary>
        /// 是否启用退订
        /// </summary>
        public bool? Enable { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public UnsubscibeType Type { get; set; }

        /// <summary>
        /// 外部 URL
        /// </summary>
        public string? ExternalUrl { get; set; }

        /// <summary>
        /// 退订按钮的 Id
        /// </summary>
        public long UnsubscribeButtonId { get; set; } = 1;

        protected override void ReadValuesFromJsons()
        {
            Enable = GetBoolValue(nameof(Enable), false);
            Type = (UnsubscibeType)GetIntValue(nameof(Type), 0);
            ExternalUrl = GetStringValue(nameof(ExternalUrl), string.Empty);
            UnsubscribeButtonId = GetLongValue(nameof(UnsubscribeButtonId), 1);
        }

        public bool IsEnable()
        {
            if (Enable == null)
                return false;
            if (Enable == false)
                return false;
            return true;
        }

        /// <summary>
        /// 退订按钮的 HTML
        /// </summary>
        public string? UnsubscribeButtonHtml { get; private set; }

        private bool _isInitialized = false;

        public async Task InitForSubscribling(SqlContextPro db, long organizationId)
        {
            if (_isInitialized)
                return;

            // 获取退订按钮
            var unsubscribeButton = await db.UnsubscribeButtons.FirstOrDefaultAsync();
            if (unsubscribeButton == null)
            {
                _logger.Warn("UnsubscribeButton 没有设置，无法添加退订");
                Enable = false;
                return;
            }

            UnsubscribeButtonHtml = unsubscribeButton.ButtonHtml;

            // 外部退订
            if (Type == UnsubscibeType.External)
            {
                // 判断是否有外部链接
                if (string.IsNullOrEmpty(ExternalUrl) || !ExternalUrl.StartsWith("http"))
                {
                    _logger.Error("ExternalUrl 为空或者格式不正确，无法添加退订");
                    Enable = false;
                    return;
                }
                return;
            }

            // 判断是否有退订页面
            var unsubscribePage = await db.UnsubscribePages.FirstOrDefaultAsync(x =>
                x.OrganizationId == organizationId
            );
            if (unsubscribePage == null)
            {
                _logger.Error($"组织 {organizationId} 未设置 UnsubscribePage, 无法添加退订");
                Enable = false;
                return;
            }
        }

        private string? _unsubscribeUrl;

        /// <summary>
        /// 获取退订链接
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public async Task<string> GetUnsubscribeUrl(SqlContext db)
        {
            if (!IsEnable())
                return string.Empty;

            if (!string.IsNullOrEmpty(_unsubscribeUrl))
                return _unsubscribeUrl;

            // 若是外部退订，直接返回外部链接
            if (Type == UnsubscibeType.External)
            {
                _unsubscribeUrl = ExternalUrl ?? string.Empty;
                return _unsubscribeUrl;
            }

            // 获取 baseUrl
            var setting = await db.AppSettings.FirstOrDefaultAsync(x =>
                x.Key == AppSetting.BaseApiUrl
            );
            if (
                setting == null
                || string.IsNullOrEmpty(setting.StringValue)
                || !setting.StringValue.StartsWith("http")
            )
            {
                _logger.Warn("BaseApiUrl 未设置或者格式错误");
                Enable = false;
                return string.Empty;
            }

            _unsubscribeUrl =
                $"{setting.StringValue.TrimEnd('/')}/pages/unsubscribe/pls-give-me-a-shot";
            return _unsubscribeUrl;
        }
    }
}
