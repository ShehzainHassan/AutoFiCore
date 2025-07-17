using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_AuctionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReserveMet",
                table: "Auctions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviewStartTime",
                table: "Auctions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReserveMetAt",
                table: "Auctions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservePrice",
                table: "Auctions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStartTime",
                table: "Auctions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReserveMet",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "PreviewStartTime",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ReserveMetAt",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ReservePrice",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                table: "Auctions");
        }
    }
}
