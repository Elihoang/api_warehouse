using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWarehouseHub.Core.Migrations
{
    /// <inheritdoc />
    public partial class TenMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "ExportReceipts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "ExportReceipts",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "ExportReceipts");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "ExportReceipts");
        }
    }
}
