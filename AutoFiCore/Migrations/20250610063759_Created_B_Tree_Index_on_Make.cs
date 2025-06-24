using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Created_B_Tree_Index_on_Make : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicle_Make_Model_Year_Include_Price`",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Make",
                table: "Vehicles",
                column: "Make");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Make",
                table: "Vehicles");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_Make_Model_Year_Include_Price`",
                table: "Vehicles",
                columns: new[] { "Make", "Model", "Year" })
                .Annotation("Npgsql:IndexInclude", new[] { "Price" });
        }
    }
}
