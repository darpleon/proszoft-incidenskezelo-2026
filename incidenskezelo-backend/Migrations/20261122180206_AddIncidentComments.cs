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
                    CommentIndex = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BodyRevision = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentComments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_IncidentComments_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "IncidentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCommentBodyChanges",
                columns: table => new
                {
                    ChangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    OldBody = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    NewBody = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCommentBodyChanges", x => x.ChangeId);
                    table.ForeignKey(
                        name: "FK_IncidentCommentBodyChanges_AppUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentCommentBodyChanges_IncidentComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "IncidentComments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCommentCreations",
                columns: table => new
                {
                    CreationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCommentCreations", x => x.CreationId);
                    table.ForeignKey(
                        name: "FK_IncidentCommentCreations_AppUser_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AppUser",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_IncidentCommentCreations_IncidentComments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "IncidentComments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommentBodyChanges_ActorUserId",
                table: "IncidentCommentBodyChanges",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommentBodyChanges_CommentId_OccurredAtUtc",
                table: "IncidentCommentBodyChanges",
                columns: new[] { "CommentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommentCreations_ActorUserId",
                table: "IncidentCommentCreations",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommentCreations_CommentId",
                table: "IncidentCommentCreations",
                column: "CommentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentComments_IncidentId_CommentIndex",
                table: "IncidentComments",
                columns: new[] { "IncidentId", "CommentIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentCommentBodyChanges");

            migrationBuilder.DropTable(
                name: "IncidentCommentCreations");

            migrationBuilder.DropTable(
                name: "IncidentComments");
        }
    }
}
