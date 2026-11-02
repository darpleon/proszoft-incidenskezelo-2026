using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIncidentLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IncidentId",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_IncidentId",
                table: "Event",
                column: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_Incidents_IncidentId",
                table: "Event",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "IncidentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_Incidents_IncidentId",
                table: "Event");

            migrationBuilder.DropIndex(
                name: "IX_Event_IncidentId",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "Event");
        }
    }
}
