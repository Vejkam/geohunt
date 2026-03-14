using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psi25_project.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyTokensAndFixModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRoundLatitude",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CurrentRoundLongitude",
                table: "Rooms");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Rooms",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Rooms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "DistanceMeters",
                table: "MultiplayerPlayers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastGuessLatitude",
                table: "MultiplayerPlayers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastGuessLongitude",
                table: "MultiplayerPlayers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MultiplayerPlayers",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<double>(
                name: "RoundLatitude",
                table: "MultiplayerGames",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RoundLongitude",
                table: "MultiplayerGames",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MultiplayerGames",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "MultiplayerGames",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "DistanceMeters",
                table: "MultiplayerPlayers");

            migrationBuilder.DropColumn(
                name: "LastGuessLatitude",
                table: "MultiplayerPlayers");

            migrationBuilder.DropColumn(
                name: "LastGuessLongitude",
                table: "MultiplayerPlayers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MultiplayerPlayers");

            migrationBuilder.DropColumn(
                name: "RoundLatitude",
                table: "MultiplayerGames");

            migrationBuilder.DropColumn(
                name: "RoundLongitude",
                table: "MultiplayerGames");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MultiplayerGames");

            migrationBuilder.DropColumn(
                name: "State",
                table: "MultiplayerGames");

            migrationBuilder.AddColumn<double>(
                name: "CurrentRoundLatitude",
                table: "Rooms",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentRoundLongitude",
                table: "Rooms",
                type: "double precision",
                nullable: true);
        }
    }
}
