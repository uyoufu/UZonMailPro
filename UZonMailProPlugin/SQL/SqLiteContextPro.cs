using Microsoft.EntityFrameworkCore;

namespace UZonMailProPlugin.SQL
{
    public class SqLiteContextPro : SqlContextPro
    {
        #region 初始化
        public SqLiteContextPro() { }
        public SqLiteContextPro(DbContextOptions options) : base(options)
        {
        }
        #endregion
    }
}
