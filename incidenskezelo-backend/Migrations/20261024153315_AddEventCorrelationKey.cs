using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCorrelationKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationKey",
                table: "Event",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationKey",
                table: "Event");
        }
    }
}
