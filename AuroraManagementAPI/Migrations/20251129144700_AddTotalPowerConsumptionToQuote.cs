using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuroraManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalPowerConsumptionToQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Section",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "ButtonCount",
                table: "Devices",
                newName: "PowerConsumption");

            migrationBuilder.AddColumn<int>(
                name: "TotalPowerConsumption",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 29, 14, 46, 59, 173, DateTimeKind.Utc).AddTicks(7350));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPowerConsumption",
                table: "Quotes");

            migrationBuilder.RenameColumn(
                name: "PowerConsumption",
                table: "Devices",
                newName: "ButtonCount");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "Devices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 27, 17, 5, 26, 454, DateTimeKind.Utc).AddTicks(3422));

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 1,
                column: "Section",
                value: "Main");

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 2,
                column: "Section",
                value: "Input");
        }
    }
}
