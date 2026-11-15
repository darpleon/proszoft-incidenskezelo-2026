using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentComments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_IncidentComments_AppUser_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentComments_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentComments_AuthorUserId",
                table: "IncidentComments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentComments_IncidentId_OccurredAtUtc",
                table: "IncidentComments",
                columns: new[] { "IncidentId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentComments");
        }
    }
}
