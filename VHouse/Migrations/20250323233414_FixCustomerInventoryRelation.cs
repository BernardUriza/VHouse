using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerInventoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_CustomerId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CustomerId",
                table: "Inventories",
                column: "CustomerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventories_CustomerId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CustomerId",
                table: "Inventories",
                column: "CustomerId");
        }
    }
}
