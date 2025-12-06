using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using UZonMail.DB.SQL;
using UZonMail.DB.SqLite;
using UZonMail.DB.MySql;

namespace UZonMail.ProPlugin.SQL
{
    public class MySqlContextFactory : IDesignTimeDbContextFactory<MySqlContextPro>
    {
        public MySqlContextPro CreateDbContext(string[] args)
        {
            Batteries.Init();

            var connection = new MySqlConnectionConfig()
            {
                Database = "uzon-mail",
                Enable = true,
                Host = "127.0.0.1",
                Password = "uzon-mail",
                Port = 3306,
                Version = "8.4.0.0",
                User = "uzon-mail"
            };

            var optionsBuilder = new DbContextOptionsBuilder<SqlContextPro>();
            optionsBuilder.UseMySql(connection.ConnectionString, new MySqlServerVersion(connection.MysqlVersion));

            return new MySqlContextPro(optionsBuilder.Options);
        }
    }
}
