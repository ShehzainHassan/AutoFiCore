using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Created_Vehicle_Features_Table_For_AI_Understanding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleFeatures",
                columns: table => new
                {
                    Make = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Drivetrain = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Engine = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FuelEconomy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Performance = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Measurements = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Options = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleFeatures", x => new { x.Make, x.Model, x.Year });
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFeatures_Drivetrain",
                table: "VehicleFeatures",
                column: "Drivetrain");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFeatures_Engine",
                table: "VehicleFeatures",
                column: "Engine");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFeatures_Make",
                table: "VehicleFeatures",
                column: "Make");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFeatures_Model",
                table: "VehicleFeatures",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleFeatures_Year",
                table: "VehicleFeatures",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleFeatures");
        }
    }
}
