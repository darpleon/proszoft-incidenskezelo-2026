using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentActorAndAssignee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActorUserId",
                table: "IncidentTitleChanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActorUserId",
                table: "IncidentStatusChanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssigneeRevision",
                table: "Incidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AssigneeUserId",
                table: "Incidents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActorUserId",
                table: "IncidentPriorityChanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActorUserId",
                table: "IncidentDescriptionChanges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActorUserId",
                table: "IncidentCreations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IncidentAssigneeChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<int>(type: "int", nullable: true),
                    NewValue = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentAssigneeChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentAssigneeChanges_AppUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentAssigneeChanges_AppUser_NewValue",
                        column: x => x.NewValue,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentAssigneeChanges_AppUser_OldValue",
                        column: x => x.OldValue,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentAssigneeChanges_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTitleChanges_ActorUserId",
                table: "IncidentTitleChanges",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentStatusChanges_ActorUserId",
                table: "IncidentStatusChanges",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AssigneeUserId",
                table: "Incidents",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentPriorityChanges_ActorUserId",
                table: "IncidentPriorityChanges",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentDescriptionChanges_ActorUserId",
                table: "IncidentDescriptionChanges",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCreations_ActorUserId",
                table: "IncidentCreations",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAssigneeChanges_ActorUserId",
                table: "IncidentAssigneeChanges",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAssigneeChanges_IncidentId_OccurredAtUtc",
                table: "IncidentAssigneeChanges",
                columns: new[] { "IncidentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAssigneeChanges_NewValue",
                table: "IncidentAssigneeChanges",
                column: "NewValue");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAssigneeChanges_OldValue",
                table: "IncidentAssigneeChanges",
                column: "OldValue");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentCreations_AppUser_ActorUserId",
                table: "IncidentCreations",
                column: "ActorUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentDescriptionChanges_AppUser_ActorUserId",
                table: "IncidentDescriptionChanges",
                column: "ActorUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentPriorityChanges_AppUser_ActorUserId",
                table: "IncidentPriorityChanges",
                column: "ActorUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_AppUser_AssigneeUserId",
                table: "Incidents",
                column: "AssigneeUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentStatusChanges_AppUser_ActorUserId",
                table: "IncidentStatusChanges",
                column: "ActorUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IncidentTitleChanges_AppUser_ActorUserId",
                table: "IncidentTitleChanges",
                column: "ActorUserId",
                principalTable: "AppUser",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IncidentCreations_AppUser_ActorUserId",
                table: "IncidentCreations");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentDescriptionChanges_AppUser_ActorUserId",
                table: "IncidentDescriptionChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentPriorityChanges_AppUser_ActorUserId",
                table: "IncidentPriorityChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_AppUser_AssigneeUserId",
                table: "Incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentStatusChanges_AppUser_ActorUserId",
                table: "IncidentStatusChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_IncidentTitleChanges_AppUser_ActorUserId",
                table: "IncidentTitleChanges");

            migrationBuilder.DropTable(
                name: "IncidentAssigneeChanges");

            migrationBuilder.DropIndex(
                name: "IX_IncidentTitleChanges_ActorUserId",
                table: "IncidentTitleChanges");

            migrationBuilder.DropIndex(
                name: "IX_IncidentStatusChanges_ActorUserId",
                table: "IncidentStatusChanges");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_AssigneeUserId",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_IncidentPriorityChanges_ActorUserId",
                table: "IncidentPriorityChanges");

            migrationBuilder.DropIndex(
                name: "IX_IncidentDescriptionChanges_ActorUserId",
                table: "IncidentDescriptionChanges");

            migrationBuilder.DropIndex(
                name: "IX_IncidentCreations_ActorUserId",
                table: "IncidentCreations");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "IncidentTitleChanges");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "IncidentStatusChanges");

            migrationBuilder.DropColumn(
                name: "AssigneeRevision",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "AssigneeUserId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "IncidentPriorityChanges");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "IncidentDescriptionChanges");

            migrationBuilder.DropColumn(
                name: "ActorUserId",
                table: "IncidentCreations");
        }
    }
}
