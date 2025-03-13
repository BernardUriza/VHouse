using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1️⃣ Allow NULL values temporarily
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "OrderItem",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderItem",
                type: "integer",
                nullable: false,
                defaultValue: 1); // ✅ Set default quantity

            // 2️⃣ Ensure existing records don't break by setting a default valid ProductId
            migrationBuilder.Sql(
                "UPDATE \"OrderItem\" SET \"ProductId\" = (SELECT \"ProductId\" FROM \"Products\" LIMIT 1) WHERE \"ProductId\" IS NULL;"
            );

            // 3️⃣ Now enforce NOT NULL constraint
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "OrderItem",
                type: "integer",
                nullable: false);

            // 4️⃣ Add the Foreign Key Constraint after ensuring valid values
            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductId",
                table: "OrderItem",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Products_ProductId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_ProductId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderItem");
        }
    }
}
