using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Service",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Service", x => x.ServiceId);
                    table.CheckConstraint("CK_Service_Kind", "[Kind] IN ('WebApp', 'Api', 'Database', 'Worker', 'External')");
                });

            migrationBuilder.CreateTable(
                name: "Event",
                columns: table => new
                {
                    EventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceEventId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Event", x => x.EventId);
                    table.CheckConstraint("CK_Event_EventType", "[EventType] IN ('ServiceDown', 'ServiceRecovered', 'HighLatency', 'HighErrorRate', 'DbConnectionError', 'JobFailed', 'CapacityIssue', 'DependencyFailure')");
                    table.CheckConstraint("CK_Event_PayloadJson", "[PayloadJson] IS NULL OR ISJSON([PayloadJson]) = 1");
                    table.CheckConstraint("CK_Event_Severity", "[Severity] IN ('Info', 'Warning', 'Error', 'Critical')");
                    table.ForeignKey(
                        name: "FK_Event_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "ServiceId");
                });

            migrationBuilder.InsertData(
                table: "Service",
                columns: new[] { "ServiceId", "Code", "IsActive", "Kind", "Name" },
                values: new object[,]
                {
                    { 1, "postgres-main", true, "Database", "Main database" },
                    { 2, "orders-api", true, "Api", "Orders API" },
                    { 3, "batch-worker", true, "Worker", "Batch worker" },
                    { 4, "web-portal", true, "WebApp", "Web portal" },
                    { 5, "file-ingest", true, "Worker", "File ingest" },
                    { 6, "payment-gw", true, "External", "Payment gateway" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Event_ServiceId",
                table: "Event",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_SourceEventId",
                table: "Event",
                column: "SourceEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Service_Code",
                table: "Service",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Service");
        }
    }
}
