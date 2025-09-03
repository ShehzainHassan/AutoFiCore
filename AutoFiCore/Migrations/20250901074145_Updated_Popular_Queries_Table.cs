using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Popular_Queries_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PopularQueries_LastAsked",
                table: "PopularQueries");

            migrationBuilder.DropIndex(
                name: "IX_PopularQueries_NormalizedQuery",
                table: "PopularQueries");

            migrationBuilder.DropColumn(
                name: "NormalizedQuery",
                table: "PopularQueries");

            migrationBuilder.AddColumn<float[]>(
                name: "Embedding",
                table: "PopularQueries",
                type: "real[]",
                nullable: false,
                defaultValue: new float[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PopularQueries_Count_LastAsked",
                table: "PopularQueries",
                columns: new[] { "Count", "LastAsked" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PopularQueries_Count_LastAsked",
                table: "PopularQueries");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "PopularQueries");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedQuery",
                table: "PopularQueries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_PopularQueries_LastAsked",
                table: "PopularQueries",
                column: "LastAsked");

            migrationBuilder.CreateIndex(
                name: "IX_PopularQueries_NormalizedQuery",
                table: "PopularQueries",
                column: "NormalizedQuery",
                unique: true);
        }
    }
}
