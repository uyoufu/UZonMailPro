using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UzonMail.ProPlugin.Migrations.PostgreSql
{
    /// <inheritdoc />
    public partial class fixAnchorCallbackFail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MxDomainCaches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    AcceptsMail = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MxDomainCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxVerificationSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SyntaxDomain = table.Column<string>(type: "text", nullable: true),
                    SyntaxUsername = table.Column<string>(type: "text", nullable: true),
                    SyntaxSuggestion = table.Column<string>(type: "text", nullable: true),
                    IsValidSyntax = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisposable = table.Column<bool>(type: "boolean", nullable: false),
                    IsRoleAccount = table.Column<bool>(type: "boolean", nullable: false),
                    IsB2C = table.Column<bool>(type: "boolean", nullable: false),
                    MxDomainCacheId = table.Column<long>(type: "bigint", nullable: true),
                    CanConnectSmtp = table.Column<bool>(type: "boolean", nullable: false),
                    HasFullInbox = table.Column<bool>(type: "boolean", nullable: false),
                    IsCatchAll = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeliverable = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxVerificationSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboxVerificationSnapshots_MxDomainCaches_MxDomainCacheId",
                        column: x => x.MxDomainCacheId,
                        principalTable: "MxDomainCaches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MxDomainRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MxDomainCacheId = table.Column<long>(type: "bigint", nullable: false),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    _id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MxDomainRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MxDomainRecords_MxDomainCaches_MxDomainCacheId",
                        column: x => x.MxDomainCacheId,
                        principalTable: "MxDomainCaches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxVerificationSnapshots_MxDomainCacheId",
                table: "InboxVerificationSnapshots",
                column: "MxDomainCacheId");

            migrationBuilder.CreateIndex(
                name: "IX_InboxVerificationSnapshots_NormalizedEmail",
                table: "InboxVerificationSnapshots",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MxDomainCaches_Domain",
                table: "MxDomainCaches",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MxDomainRecords_MxDomainCacheId",
                table: "MxDomainRecords",
                column: "MxDomainCacheId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxVerificationSnapshots");

            migrationBuilder.DropTable(
                name: "MxDomainRecords");

            migrationBuilder.DropTable(
                name: "MxDomainCaches");
        }
    }
}
