using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UZonMailProPlugin.Migrations.PostgreSQL
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
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Subjects = table.Column<List<string>>(type: "text[]", nullable: false),
                    TemplateIds = table.Column<string>(type: "text", nullable: false),
                    OutboxIds = table.Column<string>(type: "text", nullable: false),
                    InboxIds = table.Column<string>(type: "text", nullable: false),
                    CcIds = table.Column<string>(type: "text", nullable: false),
                    BccIds = table.Column<string>(type: "text", nullable: false),
                    AttachmentIds = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TasksCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpWarmUpUpPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IpWarmUpUpTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IPWarmUpPlanId = table.Column<long>(type: "bigint", nullable: false),
                    SendingGroupId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OutboxesCount = table.Column<int>(type: "integer", nullable: false),
                    InboxesCount = table.Column<int>(type: "integer", nullable: false),
                    SuccessCount = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
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
