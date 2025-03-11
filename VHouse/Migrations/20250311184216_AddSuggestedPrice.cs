using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestedPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceSuggested",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceSuggested",
                table: "Products");
        }
    }
}
