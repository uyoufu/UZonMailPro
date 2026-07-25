using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;
using UzonMail.DB.SqLite;

namespace UzonMail.ProPlugin.SQL
{
    public class SqLiteContextPro : SqlContextPro
    {
        #region 初始化
        // 用于设计时创建 DbContext 实例
        public SqLiteContextPro(DbContextOptions<SqlContextPro> options)
            : base(options) { }

        private readonly IConfiguration? _configuration;

        [ActivatorUtilitiesConstructor]
        public SqLiteContextPro(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured)
                return;

            if (_configuration is null)
                throw new InvalidOperationException("SQLite 数据库配置不可用");

            SqlContextHelper.ConfiguringSqLite(options, _configuration);
        }
    }
}
