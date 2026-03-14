using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace psi25_project.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAchievementSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "Id",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name", "Scope" },
                values: new object[,]
                {
                    { 1, "FIRST_GUESS", "Make your first guess", true, "First Guess", 2 },
                    { 2, "BULLSEYE_100M", "Guess within 100 m", true, "Bullseye", 0 },
                    { 3, "NEAR_1KM", "Guess within 1 km", true, "Near Enough", 0 },
                    { 4, "SCORE_10K", "Score 10,000+ points in a game", true, "Five Digits", 1 },
                    { 5, "CLEAN_SWEEP", "All rounds in a game are <= 1 km distance", true, "Clean Sweep", 1 }
                });
        }
    }
}
