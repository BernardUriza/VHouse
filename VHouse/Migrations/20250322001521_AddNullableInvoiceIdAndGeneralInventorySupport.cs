using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddNullableInvoiceIdAndGeneralInventorySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId",
                table: "InventoryItems");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "InventoryItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId1",
                table: "InventoryItems",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_InvoiceId1",
                table: "InventoryItems",
                column: "InvoiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId",
                table: "InventoryItems",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId1",
                table: "InventoryItems",
                column: "InvoiceId1",
                principalTable: "Invoices",
                principalColumn: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId1",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_InvoiceId1",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "InvoiceId1",
                table: "InventoryItems");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "InventoryItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Invoices_InvoiceId",
                table: "InventoryItems",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
