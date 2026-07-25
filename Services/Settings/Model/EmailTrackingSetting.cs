using UzonMail.CorePlugin.Services.Settings.Model;

namespace UzonMail.ProPlugin.Services.Settings.Model
{
    /// <summary>
    /// 邮件跟踪设置
    /// </summary>
    public class EmailTrackingSetting : BaseSettingModel
    {
        /// <summary>
        /// 发送邮件跟踪器
        /// </summary>
        public bool? EnableEmailTracker { get; set; }

        protected override void ReadValuesFromJson()
        {
            EnableEmailTracker = GetBoolValue(nameof(EnableEmailTracker), false);
        }

        public bool IsEnableTracker()
        {
            if (EnableEmailTracker == null)
                return false;
            if (EnableEmailTracker == false)
                return false;
            return true;
        }
    }
}
