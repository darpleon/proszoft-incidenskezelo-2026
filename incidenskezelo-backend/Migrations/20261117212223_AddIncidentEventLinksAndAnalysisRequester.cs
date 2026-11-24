using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentEventLinksAndAnalysisRequester : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestedByUserId",
                table: "IncidentAnalysis",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IncidentEventLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentEventLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentEventLinks_AppUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentEventLinks_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "EventId");
                    table.ForeignKey(
                        name: "FK_IncidentEventLinks_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAnalysis_RequestedByUserId",
                table: "IncidentAnalysis",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEventLinks_ActorUserId",
                table: "IncidentEventLinks",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEventLinks_EventId",
                table: "IncidentEventLinks",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentEventLinks_IncidentId_OccurredAtUtc",
                table: "IncidentEventLinks",
                columns: new[] { "IncidentId", "OccurredAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentAnalysis_AppUser_RequestedByUserId",
                table: "IncidentAnalysis",
                column: "RequestedByUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentAnalysis_AppUser_RequestedByUserId",
                table: "IncidentAnalysis");

            migrationBuilder.DropTable(
                name: "IncidentEventLinks");

            migrationBuilder.DropIndex(
                name: "IX_IncidentAnalysis_RequestedByUserId",
                table: "IncidentAnalysis");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "IncidentAnalysis");
        }
    }
}
