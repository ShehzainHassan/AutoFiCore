using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedSchemaForCarFeaturesJSON : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TorqueFtLBS",
                table: "Engines",
                newName: "torque_ft_lbs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "torque_ft_lbs",
                table: "Engines",
                newName: "TorqueFtLBS");
        }
    }
}
