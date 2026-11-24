using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RejectedEvent",
                columns: table => new
                {
                    RejectedEventId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RejectedEvent", x => x.RejectedEventId);
                    table.CheckConstraint("CK_RejectedEvent_Reason", "[Reason] IN ('Malformed', 'Invalid')");
                    table.CheckConstraint("CK_RejectedEvent_Source", "[Source] IN ('Queue', 'Http')");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RejectedEvent");
        }
    }
}
