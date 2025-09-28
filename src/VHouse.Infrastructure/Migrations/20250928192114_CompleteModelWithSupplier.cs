using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteModelWithSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5430));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5434));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5436));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5439));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5441));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5444));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 19, 21, 13, 786, DateTimeKind.Utc).AddTicks(5446));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1702));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1704));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1705));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1707));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1708));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1709));

            migrationBuilder.UpdateData(
                table: "Albums",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 9, 26, 22, 27, 39, 527, DateTimeKind.Utc).AddTicks(1711));
        }
    }
}
