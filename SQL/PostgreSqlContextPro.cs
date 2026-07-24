using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL;

namespace UzonMail.ProPlugin.SQL
{
    public class PostgreSqlContextPro : SqlContextPro
    {
        private readonly IConfiguration _configuration;

        internal PostgreSqlContextPro(DbContextOptions<SqlContextPro> options) : base(options)
        {
        }

        [ActivatorUtilitiesConstructor]
        public PostgreSqlContextPro(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            SqlContextHelper.ConfiguringPostgreSql(options, _configuration);
        }
    }
}
