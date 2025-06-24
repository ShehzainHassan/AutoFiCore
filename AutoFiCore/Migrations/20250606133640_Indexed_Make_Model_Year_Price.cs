using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Indexed_Make_Model_Year_Price : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_Make_Model_Year_Include_Price`",
                table: "Vehicles",
                columns: new[] { "Make", "Model", "Year" })
                .Annotation("Npgsql:IndexInclude", new[] { "Price" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicle_Make_Model_Year_Include_Price`",
                table: "Vehicles");
        }
    }
}
