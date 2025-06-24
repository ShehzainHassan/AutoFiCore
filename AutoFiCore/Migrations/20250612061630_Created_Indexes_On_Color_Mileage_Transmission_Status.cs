using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Created_Indexes_On_Color_Mileage_Transmission_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Color",
                table: "Vehicles",
                column: "Color");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Mileage",
                table: "Vehicles",
                column: "Mileage");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Status",
                table: "Vehicles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Transmission",
                table: "Vehicles",
                column: "Transmission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Color",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Mileage",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Status",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Transmission",
                table: "Vehicles");
        }
    }
}
