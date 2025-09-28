using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2761));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2764));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2767));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2769));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2771));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2774));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 44, 8, 903, DateTimeKind.Utc).AddTicks(2776));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3941));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3944));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3947));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3949));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3951));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3953));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 20, 27, 7, 335, DateTimeKind.Utc).AddTicks(3956));
        }
    }
}
