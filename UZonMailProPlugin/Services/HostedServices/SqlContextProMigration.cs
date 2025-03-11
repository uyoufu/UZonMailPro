using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.SQL;
using UZonMailService.Services.PostStartup;

namespace UZonMailProPlugin.Services.HostedServices
{
    public class SqlContextProMigration(SqlContextPro db) : IHostedServiceStart, IScopedService<IHostedServiceStart>
    {
        public int Order => -1;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 数据库迁移
            db.Database.Migrate();
            await db.Database.EnsureCreatedAsync(stoppingToken);
        }
    }
}
