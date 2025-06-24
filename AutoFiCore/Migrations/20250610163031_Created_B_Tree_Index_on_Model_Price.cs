using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Created_B_Tree_Index_on_Model_Price : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_Make_Id",
                table: "Vehicles",
                newName: "IX_Vehicles_Price_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Model",
                table: "Vehicles",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Price",
                table: "Vehicles",
                column: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Model",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Price",
                table: "Vehicles");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_Price_Id",
                table: "Vehicles",
                newName: "IX_Vehicles_Make_Id");
        }
    }
}
