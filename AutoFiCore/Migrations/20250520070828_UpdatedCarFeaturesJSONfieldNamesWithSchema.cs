using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCarFeaturesJSONfieldNamesWithSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BetterCapacityKWH",
                table: "FuelEconomies");

            migrationBuilder.DropColumn(
                name: "RangeMiles",
                table: "FuelEconomies");

            migrationBuilder.RenameColumn(
                name: "torque_ft_lbs",
                table: "Engines",
                newName: "TorqueFtLBS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TorqueFtLBS",
                table: "Engines",
                newName: "torque_ft_lbs");

            migrationBuilder.AddColumn<int>(
                name: "BetterCapacityKWH",
                table: "FuelEconomies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RangeMiles",
                table: "FuelEconomies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
