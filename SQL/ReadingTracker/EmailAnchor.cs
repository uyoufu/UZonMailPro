using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.ProPlugin.SQL;

namespace UzonMail.ProPlugin.SQL.ReadingTracker
{
    /// <summary>
    /// 邮箱锚点
    /// </summary>
    public class EmailAnchor : SqlId, IEntityTypeConfiguration<EmailAnchor>
    {
        /// <summary>
        /// 所属用户
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 发件组 id
        /// </summary>
        public long SendingGroupId { get; set; }

        /// <summary>
        /// 发件 id
        /// </summary>
        public long SendingItemId { get; set; }

        /// <summary>
        /// 发件箱邮箱
        /// </summary>
        public string OutboxEmail { get; set; } = string.Empty;

        /// <summary>
        /// 邮件箱邮箱
        /// </summary>
        public string InboxEmails { get; set; } = string.Empty;

        /// <summary>
        /// 访问数量
        /// </summary>
        public int VisitedCount { get; set; }

        /// <summary>
        /// 第一次访问日期
        /// </summary>
        public DateTime FirstVisitDate { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// 最近一次访问日期
        /// </summary>
        public DateTime LastVisitDate { get; set; }

        /// <summary>
        /// 访问历史
        /// </summary>
        public List<EmailVisitHistory> VisitedHistories { get; set; } = [];

        /// <summary>
        /// 添加 ef 配置
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(EntityTypeBuilder<EmailAnchor> builder)
        {
            builder.HasMany(x => x.VisitedHistories).WithMany();
        }
    }
}
