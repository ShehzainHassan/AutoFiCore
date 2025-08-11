using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Error_Log_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_ErrorType",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ErrorType",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "StackTrace",
                table: "ErrorLogs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ErrorLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "ErrorCode",
                table: "ErrorLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_ErrorCode",
                table: "ErrorLogs",
                column: "ErrorCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ErrorLogs_ErrorCode",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "ErrorLogs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "ErrorLogs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "ErrorType",
                table: "ErrorLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StackTrace",
                table: "ErrorLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ErrorLogs_ErrorType",
                table: "ErrorLogs",
                column: "ErrorType");
        }
    }
}
