using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.ProPlugin.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class addIpWarmUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IpWarmUpUpPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Subjects = table.Column<string>(type: "TEXT", nullable: false),
                    TemplateIds = table.Column<string>(type: "TEXT", nullable: false),
                    OutboxIds = table.Column<string>(type: "TEXT", nullable: false),
                    InboxIds = table.Column<string>(type: "TEXT", nullable: false),
                    CcIds = table.Column<string>(type: "TEXT", nullable: false),
                    BccIds = table.Column<string>(type: "TEXT", nullable: false),
                    AttachmentIds = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TasksCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpWarmUpUpPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IpWarmUpUpTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IPWarmUpPlanId = table.Column<long>(type: "INTEGER", nullable: false),
                    SendingGroupId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OutboxesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    InboxesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    _id = table.Column<string>(type: "TEXT", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpWarmUpUpTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IpWarmUpUpTasks_IpWarmUpUpPlans_IPWarmUpPlanId",
                        column: x => x.IPWarmUpPlanId,
                        principalTable: "IpWarmUpUpPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IpWarmUpUpTasks_IPWarmUpPlanId",
                table: "IpWarmUpUpTasks",
                column: "IPWarmUpPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IpWarmUpUpTasks");

            migrationBuilder.DropTable(
                name: "IpWarmUpUpPlans");
        }
    }
}
