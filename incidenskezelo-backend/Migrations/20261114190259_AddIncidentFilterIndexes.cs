using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentFilterIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Event_IncidentId",
                table: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Priority",
                table: "Incidents",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_Status",
                table: "Incidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCreations_OccurredAtUtc",
                table: "IncidentCreations",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Event_IncidentId_ServiceId",
                table: "Event",
                columns: new[] { "IncidentId", "ServiceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Incidents_Priority",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_Status",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_IncidentCreations_OccurredAtUtc",
                table: "IncidentCreations");

            migrationBuilder.DropIndex(
                name: "IX_Event_IncidentId_ServiceId",
                table: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_Event_IncidentId",
                table: "Event",
                column: "IncidentId");
        }
    }
}
