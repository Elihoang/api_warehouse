using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeWarehouseHub.Core.Migrations
{
    /// <inheritdoc />
    public partial class newndb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "ImportDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BatchId",
                table: "ExportDetails",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AutoReorderSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MinStockLevel = table.Column<int>(type: "integer", nullable: false),
                    ReorderPoint = table.Column<int>(type: "integer", nullable: false),
                    ReorderQuantity = table.Column<int>(type: "integer", nullable: false),
                    MaxStockLevel = table.Column<int>(type: "integer", nullable: false),
                    IsAutoReorderEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoReorderSettings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_AutoReorderSettings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AutoReorderSettings_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemandForecasts",
                columns: table => new
                {
                    ForecastId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ForecastPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PredictedDemand = table.Column<int>(type: "integer", nullable: false),
                    ActualDemand = table.Column<int>(type: "integer", nullable: false),
                    Accuracy = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Algorithm = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecommendedOrderQuantity = table.Column<int>(type: "integer", nullable: false),
                    SuggestedOrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandForecasts", x => x.ForecastId);
                    table.ForeignKey(
                        name: "FK_DemandForecasts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DemandForecasts_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAudits",
                columns: table => new
                {
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAudits", x => x.AuditId);
                    table.ForeignKey(
                        name: "FK_InventoryAudits_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAudits_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductBatches",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CostPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_ProductBatches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBatches_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "WarehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAuditDetails",
                columns: table => new
                {
                    AuditDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystemQuantity = table.Column<int>(type: "integer", nullable: false),
                    ActualQuantity = table.Column<int>(type: "integer", nullable: false),
                    Variance = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAuditDetails", x => x.AuditDetailId);
                    table.ForeignKey(
                        name: "FK_InventoryAuditDetails_InventoryAudits_AuditId",
                        column: x => x.AuditId,
                        principalTable: "InventoryAudits",
                        principalColumn: "AuditId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAuditDetails_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAuditDetails_Users_AuditedByUserId",
                        column: x => x.AuditedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportDetails_BatchId",
                table: "ImportDetails",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ExportDetails_BatchId",
                table: "ExportDetails",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoReorderSettings_ProductId",
                table: "AutoReorderSettings",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoReorderSettings_WarehouseId",
                table: "AutoReorderSettings",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandForecasts_ProductId",
                table: "DemandForecasts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandForecasts_WarehouseId",
                table: "DemandForecasts",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditDetails_AuditedByUserId",
                table: "InventoryAuditDetails",
                column: "AuditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditDetails_AuditId",
                table: "InventoryAuditDetails",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAuditDetails_ProductId",
                table: "InventoryAuditDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_CreatedByUserId",
                table: "InventoryAudits",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAudits_WarehouseId",
                table: "InventoryAudits",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_ProductId",
                table: "ProductBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_WarehouseId",
                table: "ProductBatches",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExportDetails_ProductBatches_BatchId",
                table: "ExportDetails",
                column: "BatchId",
                principalTable: "ProductBatches",
                principalColumn: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportDetails_ProductBatches_BatchId",
                table: "ImportDetails",
                column: "BatchId",
                principalTable: "ProductBatches",
                principalColumn: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExportDetails_ProductBatches_BatchId",
                table: "ExportDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportDetails_ProductBatches_BatchId",
                table: "ImportDetails");

            migrationBuilder.DropTable(
                name: "AutoReorderSettings");

            migrationBuilder.DropTable(
                name: "DemandForecasts");

            migrationBuilder.DropTable(
                name: "InventoryAuditDetails");

            migrationBuilder.DropTable(
                name: "ProductBatches");

            migrationBuilder.DropTable(
                name: "InventoryAudits");

            migrationBuilder.DropIndex(
                name: "IX_ImportDetails_BatchId",
                table: "ImportDetails");

            migrationBuilder.DropIndex(
                name: "IX_ExportDetails_BatchId",
                table: "ExportDetails");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "ImportDetails");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "ExportDetails");
        }
    }
}
