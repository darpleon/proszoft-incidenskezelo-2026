using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentRevisionCountersAndPublicIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DescriptionRevision",
                table: "Incidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PriorityRevision",
                table: "Incidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusRevision",
                table: "Incidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TitleRevision",
                table: "Incidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE Incidents SET
                    PublicId = NEWID(),
                    TitleRevision = (SELECT COUNT(*) FROM IncidentTitleChanges c WHERE c.IncidentId = Incidents.IncidentId),
                    DescriptionRevision = (SELECT COUNT(*) FROM IncidentDescriptionChanges c WHERE c.IncidentId = Incidents.IncidentId),
                    PriorityRevision = (SELECT COUNT(*) FROM IncidentPriorityChanges c WHERE c.IncidentId = Incidents.IncidentId),
                    StatusRevision = (SELECT COUNT(*) FROM IncidentStatusChanges c WHERE c.IncidentId = Incidents.IncidentId);
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_PublicId",
                table: "Incidents",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Incidents_PublicId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "DescriptionRevision",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "PriorityRevision",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "StatusRevision",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "TitleRevision",
                table: "Incidents");
        }
    }
}
