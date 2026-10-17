using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentCreationAndFieldHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM IncidentStatusChanges WHERE FromStatus IS NULL");

            migrationBuilder.RenameColumn(
                name: "FromStatus",
                table: "IncidentStatusChanges",
                newName: "OldValue");

            migrationBuilder.RenameColumn(
                name: "ToStatus",
                table: "IncidentStatusChanges",
                newName: "NewValue");

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                table: "IncidentStatusChanges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                table: "IncidentStatusChanges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Kind",
                table: "Service",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Incidents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Incidents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Event",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "IncidentCreations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCreations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentCreations_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("INSERT INTO IncidentCreations (IncidentId, OccurredAtUtc) SELECT IncidentId, CreatedAtUtc FROM Incidents");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Incidents");

            migrationBuilder.CreateTable(
                name: "IncidentDescriptionChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentDescriptionChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentDescriptionChanges_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentPriorityChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentPriorityChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentPriorityChanges_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentTitleChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentTitleChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentTitleChanges_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCreations_IncidentId",
                table: "IncidentCreations",
                column: "IncidentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentDescriptionChanges_IncidentId_OccurredAtUtc",
                table: "IncidentDescriptionChanges",
                columns: new[] { "IncidentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentPriorityChanges_IncidentId_OccurredAtUtc",
                table: "IncidentPriorityChanges",
                columns: new[] { "IncidentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTitleChanges_IncidentId_OccurredAtUtc",
                table: "IncidentTitleChanges",
                columns: new[] { "IncidentId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentCreations");

            migrationBuilder.DropTable(
                name: "IncidentDescriptionChanges");

            migrationBuilder.DropTable(
                name: "IncidentPriorityChanges");

            migrationBuilder.DropTable(
                name: "IncidentTitleChanges");

            migrationBuilder.RenameColumn(
                name: "NewValue",
                table: "IncidentStatusChanges",
                newName: "ToStatus");

            migrationBuilder.RenameColumn(
                name: "OldValue",
                table: "IncidentStatusChanges",
                newName: "FromStatus");

            migrationBuilder.AlterColumn<string>(
                name: "Kind",
                table: "Service",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FromStatus",
                table: "IncidentStatusChanges",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Incidents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Incidents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Event",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
