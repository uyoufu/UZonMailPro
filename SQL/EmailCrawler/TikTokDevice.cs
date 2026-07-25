using UzonMail.DB.SQL.Base;

namespace UzonMail.ProPlugin.SQL.EmailCrawler
{
    public class TikTokDevice : UserAndOrgId
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        public string OdinId { get; set; } = string.Empty;

        /// <summary>
        /// 是否共享
        /// 为 true 时，要同时设置 organizationId
        /// </summary>
        public bool IsShared { get; set; }
    }
}
