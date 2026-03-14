using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace psi25_project.Migrations
{
    /// <inheritdoc />
    public partial class RoomUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRoundLatitude",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CurrentRoundLongitude",
                table: "Rooms");
        }
    }
}
