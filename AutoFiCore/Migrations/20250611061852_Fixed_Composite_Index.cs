using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Fixed_Composite_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_Price_Id",
                table: "Vehicles",
                newName: "IX_Vehicles_Make_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Model_Id",
                table: "Vehicles",
                columns: new[] { "Model", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Price_Id",
                table: "Vehicles",
                columns: new[] { "Price", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Model_Id",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Price_Id",
                table: "Vehicles");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_Make_Id",
                table: "Vehicles",
                newName: "IX_Vehicles_Price_Id");
        }
    }
}
