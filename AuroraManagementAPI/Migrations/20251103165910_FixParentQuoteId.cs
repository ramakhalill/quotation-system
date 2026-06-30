using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuroraManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixParentQuoteId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SystemType",
                table: "Projects",
                newName: "SystemTypes");

            migrationBuilder.AddColumn<int>(
                name: "ParentQuoteId",
                table: "Quotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisionNumber",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeviceCode",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SupplierDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProfitRatio = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ActualPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSystemTypes",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SystemTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSystemTypes", x => new { x.ProjectId, x.SystemTypeId });
                    table.ForeignKey(
                        name: "FK_ProjectSystemTypes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSystemTypes_SystemTypes_SystemTypeId",
                        column: x => x.SystemTypeId,
                        principalTable: "SystemTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 16, 59, 9, 424, DateTimeKind.Utc).AddTicks(8509));

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DeviceCode", "SystemType" },
                values: new object[] { "", "KNX" });

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DeviceCode", "SystemType" },
                values: new object[] { "", "KNX" });

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                column: "SystemTypes",
                value: "");

            migrationBuilder.InsertData(
                table: "SystemTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "KNX" },
                    { 2, "BusPro" },
                    { 3, "Wireless" },
                    { 4, "Low Current" },
                    { 5, "Smart Wi-Fi" }
                });

            

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_ParentQuoteId",
                table: "Quotes",
                column: "ParentQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSystemTypes_SystemTypeId",
                table: "ProjectSystemTypes",
                column: "SystemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDevices_DeviceId",
                table: "SupplierDevices",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Quotes_ParentQuoteId",
                table: "Quotes",
                column: "ParentQuoteId",
                principalTable: "Quotes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Quotes_ParentQuoteId",
                table: "Quotes");

            migrationBuilder.DropTable(
                name: "ProjectSystemTypes");

            migrationBuilder.DropTable(
                name: "SupplierDevices");

            migrationBuilder.DropTable(
                name: "SystemTypes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_ParentQuoteId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "ParentQuoteId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "RevisionNumber",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "DeviceCode",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "SystemTypes",
                table: "Projects",
                newName: "SystemType");

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 14, 11, 7, 57, 773, DateTimeKind.Utc).AddTicks(9419));

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 1,
                column: "SystemType",
                value: "Lighting");

            migrationBuilder.UpdateData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: 2,
                column: "SystemType",
                value: "Lighting");

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Id",
                keyValue: 1,
                column: "SystemType",
                value: "Lighting");
        }
    }
}
