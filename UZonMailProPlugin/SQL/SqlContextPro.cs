using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.DB.SQL.EntityConfigs;
using UZonMailProPlugin.SQL.EmailCrawler;
using UZonMailProPlugin.SQL.ReadingTracker;
using UZonMailProPlugin.SQL.Unsubscribes;

namespace UZonMailProPlugin.SQL
{
    public class SqlContextPro : SqlContextBase
    {
        #region 初始化
        public SqlContextPro() { }
        public SqlContextPro(DbContextOptions<SqlContextPro> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 调用配置
            new EntityTypeConfig().Configure(modelBuilder);
        }
        #endregion

        #region 数据表定义
        public DbSet<EmailAnchor> EmailAnchors { get; set; }
        public DbSet<EmailVisitHistory> EmailVisitHistories { get; set; }
        public DbSet<IPInfo> IPInfos { get; set; }

        // 退定相关
        public DbSet<UnsubscribePage> UnsubscribePages { get; set; }
        public DbSet<UnsubscribeEmail> UnsubscribeEmails { get; set; }
        public DbSet<UnsubscribeButton> UnsubscribeButtons { get; set; }

        // 爬虫相关
        public DbSet<CrawlerTaskInfo> CrawlerTaskInfos { get; set; } // 爬虫任务
        public DbSet<TiktokAuthor> TiktokAuthors { get; set; } // TikTok 作者信息
        public DbSet<TikTokAuthorDiversification> TikTokAuthorDiversifications { get; set; } // TikTok 作者视频分类信息
        public DbSet<CrawlerTaskResult> CrawlerTaskResults { get; set; }
        public DbSet<TikTokDevice> TikTokDevices { get; set; }
        #endregion
    }
}
