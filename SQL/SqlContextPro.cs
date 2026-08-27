using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.DB.SQL.EntityConfigs;
using UzonMail.ProPlugin.SQL.ApiAccess;
using UzonMail.ProPlugin.SQL.EmailCrawler;
using UzonMail.ProPlugin.SQL.EmailVerify;
using UzonMail.ProPlugin.SQL.IPWarmUp;
using UzonMail.ProPlugin.SQL.JsVariable;
using UzonMail.ProPlugin.SQL.ReadingTracker;
using UzonMail.ProPlugin.SQL.Unsubscribes;

namespace UzonMail.ProPlugin.SQL
{
    public class SqlContextPro : SqlContextBase
    {
        /// <summary>
        /// 运行时额外注册的外部程序集。
        /// 这些程序集中的实体会包含在运行时模型中（支持跨库查询），
        /// 但会通过 ExcludeFromMigrations 排除出本 Context 的迁移快照。
        /// 应在应用启动时（DbContext 首次构建模型之前）完成注册。
        /// </summary>
        public static List<Assembly> ExternalAssemblies { get; } = [];

        #region 初始化
        public SqlContextPro() { }

        public SqlContextPro(DbContextOptions<SqlContextPro> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 当前程序集配置（参与迁移快照）
            new EntityTypeConfig().Configure(modelBuilder);

            var currentAssembly = typeof(SqlContextPro).Assembly;
            // 将来自外部程序集的实体排除出迁移快照，由各自的 Context 负责管理其 Schema。
            // .NET 10 会把外部实体间的 many-to-many shared-type 中间表也带进迁移模型，
            // 这里一并排除，只保留当前程序集自己的实体和关联表。
            foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (!ShouldExcludeFromMigrations(entityType, currentAssembly))
                {
                    continue;
                }

                ExcludeFromMigrations(modelBuilder, entityType);
            }
        }

        private static bool ShouldExcludeFromMigrations(
            IMutableEntityType entityType,
            Assembly currentAssembly
        )
        {
            if (entityType.ClrType != typeof(Dictionary<string, object>))
            {
                return entityType.ClrType.Assembly != currentAssembly
                    && entityType.Name == entityType.ClrType.FullName;
            }

            var principalClrTypes = entityType
                .GetForeignKeys()
                .Select(x => x.PrincipalEntityType.ClrType)
                .Where(x => x != typeof(Dictionary<string, object>))
                .Distinct()
                .ToList();

            return principalClrTypes.Count > 0
                && principalClrTypes.All(x => x.Assembly != currentAssembly);
        }

        private static void ExcludeFromMigrations(
            ModelBuilder modelBuilder,
            IMutableEntityType entityType
        )
        {
            if (entityType.ClrType == typeof(Dictionary<string, object>))
            {
                modelBuilder
                    .SharedTypeEntity<Dictionary<string, object>>(entityType.Name)
                    .ToTable(t => t.ExcludeFromMigrations());
                return;
            }

            modelBuilder.Entity(entityType.ClrType).ToTable(t => t.ExcludeFromMigrations());
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

        // js 变量相关
        public DbSet<JsVariableSource> JsVariableSources { get; set; } // js 变量数据源
        public DbSet<JsFunctionDefinition> JsFunctionDefinitions { get; set; } // js 函数定义

        // 访问令牌控制
        public DbSet<AccessToken> AccessTokens { get; set; } // 访问令牌

        // 收件箱验证
        public DbSet<RecipientContactVerificationSnapshot> RecipientContactVerificationSnapshots { get; set; }
        public DbSet<MxDomainCache> MxDomainCaches { get; set; }
        public DbSet<MxDomainRecord> MxDomainRecords { get; set; }

        // ip 预热
        public DbSet<IpWarmUpUpPlan> IpWarmUpUpPlans { get; set; } // 预热计划
        public DbSet<IpWarmUpUpTask> IpWarmUpUpTasks { get; set; } // 预热具体的任务
        #endregion
    }
}
