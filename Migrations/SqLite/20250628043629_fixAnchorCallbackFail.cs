using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMail.ProPlugin.Migrations.SqLite
{
    /// <inheritdoc />
    public partial class fixAnchorCallbackFail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailVisitHistories_EmailAnchors_EmailAnchorId",
                table: "EmailVisitHistories");

            migrationBuilder.DropIndex(
                name: "IX_EmailVisitHistories_EmailAnchorId",
                table: "EmailVisitHistories");

            migrationBuilder.DropColumn(
                name: "EmailAnchorId",
                table: "EmailVisitHistories");

            migrationBuilder.CreateTable(
                name: "EmailAnchorEmailVisitHistory",
                columns: table => new
                {
                    EmailAnchorId = table.Column<long>(type: "INTEGER", nullable: false),
                    VisitedHistoriesId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAnchorEmailVisitHistory", x => new { x.EmailAnchorId, x.VisitedHistoriesId });
                    table.ForeignKey(
                        name: "FK_EmailAnchorEmailVisitHistory_EmailAnchors_EmailAnchorId",
                        column: x => x.EmailAnchorId,
                        principalTable: "EmailAnchors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailAnchorEmailVisitHistory_EmailVisitHistories_VisitedHistoriesId",
                        column: x => x.VisitedHistoriesId,
                        principalTable: "EmailVisitHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAnchorEmailVisitHistory_VisitedHistoriesId",
                table: "EmailAnchorEmailVisitHistory",
                column: "VisitedHistoriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailAnchorEmailVisitHistory");

            migrationBuilder.AddColumn<long>(
                name: "EmailAnchorId",
                table: "EmailVisitHistories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVisitHistories_EmailAnchorId",
                table: "EmailVisitHistories",
                column: "EmailAnchorId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailVisitHistories_EmailAnchors_EmailAnchorId",
                table: "EmailVisitHistories",
                column: "EmailAnchorId",
                principalTable: "EmailAnchors",
                principalColumn: "Id");
        }
    }
}
