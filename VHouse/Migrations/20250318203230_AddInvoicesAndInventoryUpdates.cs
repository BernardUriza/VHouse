using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicesAndInventoryUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Inventories_InventoryId",
                table: "InventoryItem");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItem_Products_ProductId",
                table: "InventoryItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryItem",
                table: "InventoryItem");

            migrationBuilder.RenameTable(
                name: "InventoryItem",
                newName: "InventoryItems");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryItem_ProductId",
                table: "InventoryItems",
                newName: "IX_InventoryItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryItem_InventoryId",
                table: "InventoryItems",
                newName: "IX_InventoryItems_InventoryId");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsInventoryEntry",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGeneralInventory",
                table: "Inventories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "InventoryItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "InventoryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryItems",
                table: "InventoryItems",
                column: "InventoryItemId");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderName = table.Column<string>(type: "text", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_InvoiceId",
                table: "InventoryItems",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Inventories_InventoryId",
                table: "InventoryItems",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId",
                table: "InventoryItems",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Products_ProductId",
                table: "InventoryItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Inventories_InventoryId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Products_ProductId",
                table: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryItems",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_InvoiceId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsInventoryEntry",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsGeneralInventory",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "InventoryItems");

            migrationBuilder.RenameTable(
                name: "InventoryItems",
                newName: "InventoryItem");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryItems_ProductId",
                table: "InventoryItem",
                newName: "IX_InventoryItem_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryItems_InventoryId",
                table: "InventoryItem",
                newName: "IX_InventoryItem_InventoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryItem",
                table: "InventoryItem",
                column: "InventoryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Inventories_InventoryId",
                table: "InventoryItem",
                column: "InventoryId",
                principalTable: "Inventories",
                principalColumn: "InventoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItem_Products_ProductId",
                table: "InventoryItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
