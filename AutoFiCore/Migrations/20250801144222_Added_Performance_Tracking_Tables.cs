using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Added_Performance_Tracking_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiPerformanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Endpoint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ResponseTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiPerformanceLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbQueryLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QueryType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbQueryLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ErrorType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    StackTrace = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiPerformanceLogs_Endpoint",
                table: "ApiPerformanceLogs",
                column: "Endpoint");

            migrationBuilder.CreateIndex(
                name: "IX_ApiPerformanceLogs_Timestamp",
                table: "ApiPerformanceLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_DbQueryLogs_QueryType",
                table: "DbQueryLogs",
                column: "QueryType");

            migrationBuilder.CreateIndex(
                name: "IX_DbQueryLogs_Timestamp",
                table: "DbQueryLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_ErrorType",
                table: "ErrorLogs",
                column: "ErrorType");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_Timestamp",
                table: "ErrorLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiPerformanceLogs");

            migrationBuilder.DropTable(
                name: "DbQueryLogs");

            migrationBuilder.DropTable(
                name: "ErrorLogs");
        }
    }
}
