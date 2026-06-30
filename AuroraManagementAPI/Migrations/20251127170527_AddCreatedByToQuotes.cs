using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToQuotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserName",
                table: "Quotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 27, 17, 5, 26, 454, DateTimeKind.Utc).AddTicks(3422));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserName",
                table: "Quotes");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 12, 17, 49, 18, 791, DateTimeKind.Utc).AddTicks(7483));
        }
    }
}
