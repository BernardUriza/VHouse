using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsignmentNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsignmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Terms = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TotalValueAtCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalValueAtRetail = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StorePercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BernardPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalSold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountDueToBernard = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountDueToStore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consignments_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveryNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlannedDeliveryDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DeliveryNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    InternalNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DeliveryFeedback = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DeliveryLatitude = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    DeliveryLongitude = table.Column<decimal>(type: "decimal(11,8)", nullable: true),
                    DeliveryPhotoUrls = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deliveries_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityConsigned = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantitySold = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityReturned = table.Column<int>(type: "INTEGER", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetailPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Consignments_ConsignmentId",
                        column: x => x.ConsignmentId,
                        principalTable: "Consignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignmentItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeliveryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityOrdered = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsignmentSales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsignmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsignmentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    QuantitySold = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSaleAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BernardAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaleReference = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsignmentSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsignmentSales_ConsignmentItems_ConsignmentItemId",
                        column: x => x.ConsignmentItemId,
                        principalTable: "ConsignmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsignmentSales_Consignments_ConsignmentId",
                        column: x => x.ConsignmentId,
                        principalTable: "Consignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_ConsignmentId",
                table: "ConsignmentItems",
                column: "ConsignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentItems_ProductId",
                table: "ConsignmentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_ClientTenantId",
                table: "Consignments",
                column: "ClientTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_ConsignmentDate",
                table: "Consignments",
                column: "ConsignmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_ConsignmentNumber",
                table: "Consignments",
                column: "ConsignmentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consignments_Status",
                table: "Consignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentSales_ConsignmentId",
                table: "ConsignmentSales",
                column: "ConsignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentSales_ConsignmentItemId",
                table: "ConsignmentSales",
                column: "ConsignmentItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsignmentSales_SaleDate",
                table: "ConsignmentSales",
                column: "SaleDate");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_ClientTenantId",
                table: "Deliveries",
                column: "ClientTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryDate",
                table: "Deliveries",
                column: "DeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryNumber",
                table: "Deliveries",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_Status",
                table: "Deliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_DeliveryId",
                table: "DeliveryItems",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_ProductId",
                table: "DeliveryItems",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsignmentSales");

            migrationBuilder.DropTable(
                name: "DeliveryItems");

            migrationBuilder.DropTable(
                name: "ConsignmentItems");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "Consignments");
        }
    }
}
