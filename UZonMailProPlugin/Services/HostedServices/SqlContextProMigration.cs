using Microsoft.EntityFrameworkCore;
using UZonMail.CorePlugin.Services.HostedServices;
using UZonMail.ProPlugin.SQL;

namespace UZonMail.ProPlugin.Services.HostedServices
{
    public class SqlContextProMigration(SqlContextPro db) : IScopedServiceAfterStarting
    {
        public int Order => -10000;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 数据库迁移
            db.Database.Migrate();
            await db.Database.EnsureCreatedAsync(stoppingToken);
        }
    }
}
