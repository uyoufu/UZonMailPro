﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UZonMailProPlugin.Migrations.SqLite
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
                type: "TEXT",
                nullable: false,
                defaultValue: "");
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
