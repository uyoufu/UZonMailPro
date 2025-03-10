using Microsoft.EntityFrameworkCore;

namespace UZonMailProPlugin.SQL
{
    public class MySqlContextPro: SqlContextPro
    {
        #region 初始化
        public MySqlContextPro() { }
        public MySqlContextPro(DbContextOptions options) : base(options)
        {
        }
        #endregion
    }
}
