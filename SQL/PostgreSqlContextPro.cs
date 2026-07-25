using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;

namespace UzonMail.ProPlugin.SQL
{
    public class PostgreSqlContextPro : SqlContextPro
    {
        private readonly IConfiguration? _configuration;

        internal PostgreSqlContextPro(DbContextOptions<SqlContextPro> options)
            : base(options) { }

        [ActivatorUtilitiesConstructor]
        public PostgreSqlContextPro(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured)
                return;

            if (_configuration is null)
                throw new InvalidOperationException("PostgreSQL 数据库配置不可用");

            SqlContextHelper.ConfiguringPostgreSql(options, _configuration);
        }
    }
}
