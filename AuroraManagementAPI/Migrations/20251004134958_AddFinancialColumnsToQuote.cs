using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialColumnsToQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                table: "Quotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 4, 13, 49, 58, 317, DateTimeKind.Utc).AddTicks(5157));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                table: "Quotes");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 4, 11, 35, 34, 181, DateTimeKind.Utc).AddTicks(569));
        }
    }
}
