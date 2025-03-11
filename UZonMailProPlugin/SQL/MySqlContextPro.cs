using Microsoft.EntityFrameworkCore;
using UZonMail.DB.MySql;
using UZonMail.DB.SQL;

namespace UZonMailProPlugin.SQL
{
    public class MySqlContextPro: SqlContextPro
    {
        #region 初始化
        public MySqlContextPro(DbContextOptions<SqlContextPro> options) : base(options)
        {
        }

        private readonly IConfiguration _configuration;

        [ActivatorUtilitiesConstructor]
        public MySqlContextPro(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            SqlContextHelper.ConfiguringMySql(options, _configuration);
        }
    }
}
