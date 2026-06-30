using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "Quotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Quotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Quotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InstallationFee",
                table: "Quotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 14, 38, 56, 659, DateTimeKind.Utc).AddTicks(8061));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "InstallationFee",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Quotes");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 12, 37, 39, 859, DateTimeKind.Utc).AddTicks(6546));
        }
    }
}
