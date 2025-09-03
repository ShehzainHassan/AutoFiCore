using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_Popular_Queries_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PopularQueries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NormalizedQuery = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LastAsked = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PopularQueries", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PopularQueries");
        }
    }
}
