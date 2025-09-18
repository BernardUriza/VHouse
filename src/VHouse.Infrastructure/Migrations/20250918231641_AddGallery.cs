using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlbumId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Albums",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9068), "Product catalog photos", "Products", "products", null },
                    { 2, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9075), "Customer sales receipts", "Sales Receipts", "sales-receipts", null },
                    { 3, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9078), "Supplier purchase receipts", "Purchase Receipts", "purchase-receipts", null },
                    { 4, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9082), "Client invoices and documentation", "Invoices", "invoices", null },
                    { 5, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9085), "Supplier documentation and photos", "Suppliers", "suppliers", null },
                    { 6, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9089), "Customer documentation and photos", "Customers", "customers", null },
                    { 7, new DateTime(2025, 9, 18, 23, 16, 39, 992, DateTimeKind.Utc).AddTicks(9092), "Miscellaneous photos and documents", "Misc", "misc", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Slug",
                table: "Albums",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AlbumId",
                table: "Photos",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UploadedUtc",
                table: "Photos",
                column: "UploadedUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Albums");
        }
    }
}
