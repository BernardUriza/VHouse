using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VHouse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseAuditSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    OldValues = table.Column<string>(type: "TEXT", nullable: true),
                    NewValues = table.Column<string>(type: "TEXT", nullable: true),
                    Changes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Module = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AmountInvolved = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ClientTenant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsSuccess = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlertType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClientTenant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RelatedEntity = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    AmountInvolved = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsResolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolvedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ActionData = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MetricName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MetricType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClientTenant = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RequiresAction = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActionRequired = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module",
                table: "AuditLogs",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Severity",
                table: "AuditLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_AlertType",
                table: "BusinessAlerts",
                column: "AlertType");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_ClientTenant",
                table: "BusinessAlerts",
                column: "ClientTenant");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_CreatedAt",
                table: "BusinessAlerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_IsResolved",
                table: "BusinessAlerts",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_Severity",
                table: "BusinessAlerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_ClientTenant",
                table: "SystemMetrics",
                column: "ClientTenant");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_MetricName",
                table: "SystemMetrics",
                column: "MetricName");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_MetricType",
                table: "SystemMetrics",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_Severity",
                table: "SystemMetrics",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SystemMetrics_Timestamp",
                table: "SystemMetrics",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessAlerts");

            migrationBuilder.DropTable(
                name: "SystemMetrics");
        }
    }
}
