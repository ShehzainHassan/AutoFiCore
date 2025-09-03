using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Embedding_Column_Type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double[]>(
                name: "Embedding",
                table: "PopularQueries",
                type: "double precision[]",
                nullable: false,
                oldClrType: typeof(float[]),
                oldType: "real[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float[]>(
                name: "Embedding",
                table: "PopularQueries",
                type: "real[]",
                nullable: false,
                oldClrType: typeof(double[]),
                oldType: "double precision[]");
        }
    }
}
