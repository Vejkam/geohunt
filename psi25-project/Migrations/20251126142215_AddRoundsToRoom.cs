using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psi25_project.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundsToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameInProgress",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "CurrentRound",
                table: "Rooms",
                newName: "TotalRounds");

            migrationBuilder.AddColumn<int>(
                name: "CurrentRounds",
                table: "Rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRounds",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "TotalRounds",
                table: "Rooms",
                newName: "CurrentRound");

            migrationBuilder.AddColumn<bool>(
                name: "GameInProgress",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
