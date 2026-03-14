using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psi25_project.Migrations
{
    /// <inheritdoc />
    public partial class AddCleanSweepAchievement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name", "Scope" },
                values: new object[] { 5, "CLEAN_SWEEP", "All rounds in a game are <= 1 km distance", true, "Clean Sweep", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
