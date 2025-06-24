using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Created_Composite_Index_on_Make_Model_Price_Year_Id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Make_Model_Price_Year_Id",
                table: "Vehicles",
                columns: new[] { "Make", "Model", "Price", "Year", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Make_Model_Price_Year_Id",
                table: "Vehicles");
        }
    }
}
