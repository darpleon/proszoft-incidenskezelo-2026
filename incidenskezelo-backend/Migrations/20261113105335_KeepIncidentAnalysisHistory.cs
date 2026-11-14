using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace incidenskezelo_backend.Migrations
{
    /// <inheritdoc />
    public partial class KeepIncidentAnalysisHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IncidentAnalysis",
                table: "IncidentAnalysis");

            migrationBuilder.AddColumn<long>(
                name: "AnalysisId",
                table: "IncidentAnalysis",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IncidentAnalysis",
                table: "IncidentAnalysis",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentAnalysis_IncidentId_AnalyzedAtUtc_AnalysisId",
                table: "IncidentAnalysis",
                columns: new[] { "IncidentId", "AnalyzedAtUtc", "AnalysisId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IncidentAnalysis",
                table: "IncidentAnalysis");

            migrationBuilder.DropIndex(
                name: "IX_IncidentAnalysis_IncidentId_AnalyzedAtUtc_AnalysisId",
                table: "IncidentAnalysis");

            migrationBuilder.DropColumn(
                name: "AnalysisId",
                table: "IncidentAnalysis");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IncidentAnalysis",
                table: "IncidentAnalysis",
                column: "IncidentId");
        }
    }
}
