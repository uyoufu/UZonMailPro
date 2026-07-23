using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;

namespace UZonMail.ProPlugin.SQL
{
    public class SqLiteContextPro : SqlContextPro
    {

        #region 初始化
        // 用于设计时创建 DbContext 实例
        public SqLiteContextPro(DbContextOptions<SqlContextPro> options) : base(options)
        {
        }

        private readonly IConfiguration _configuration;

        [ActivatorUtilitiesConstructor]
        public SqLiteContextPro(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            SqlContextHelper.ConfiguringSqLite(options, _configuration);
        }
    }
}
