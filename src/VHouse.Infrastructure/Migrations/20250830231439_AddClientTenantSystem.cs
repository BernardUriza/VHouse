using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientTenantSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientTenantId",
                table: "Orders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientTenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TenantName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TenantCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LoginUsername = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LoginPasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientTenantId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinOrderQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProducts_ClientTenants_ClientTenantId",
                        column: x => x.ClientTenantId,
                        principalTable: "ClientTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ClientTenantId",
                table: "Orders",
                column: "ClientTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientProducts_ClientTenantId_ProductId",
                table: "ClientProducts",
                columns: new[] { "ClientTenantId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientProducts_ProductId",
                table: "ClientProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientTenants_LoginUsername",
                table: "ClientTenants",
                column: "LoginUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientTenants_TenantCode",
                table: "ClientTenants",
                column: "TenantCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ClientTenants_ClientTenantId",
                table: "Orders",
                column: "ClientTenantId",
                principalTable: "ClientTenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ClientTenants_ClientTenantId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "ClientProducts");

            migrationBuilder.DropTable(
                name: "ClientTenants");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ClientTenantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientTenantId",
                table: "Orders");
        }
    }
}
