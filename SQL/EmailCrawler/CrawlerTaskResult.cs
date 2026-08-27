using UzonMail.DB.SQL.Base;

namespace UzonMail.ProPlugin.SQL.EmailCrawler
{
    public class CrawlerTaskResult : SqlId
    {
        /// <summary>
        /// 任务 id
        /// </summary>
        public long CrawlerTaskInfoId { get; set; }

        /// <summary>
        /// 用户 id
        /// </summary>
        public long TikTokAuthorId { get; set; }
        public TiktokAuthor TiktokAuthor { get; set; } = null!;

        /// <summary>
        /// 是否存在额外的信息
        /// </summary>
        public bool ExistExtraInfo { get; set; }

        /// <summary>
        /// 是否已转换为收件联系人
        /// </summary>
        public bool IsAttachingRecipientContact { get; set; }
    }
}
