using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailProPlugin.Migrations.Mysql
{
    /// <inheritdoc />
    public partial class addSendCountChartPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendCountChartPoints",
                table: "IpWarmUpUpPlans",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendCountChartPoints",
                table: "IpWarmUpUpPlans");
        }
    }
}
