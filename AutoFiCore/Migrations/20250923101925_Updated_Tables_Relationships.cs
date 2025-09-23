using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Tables_Relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Auctions_AuctionId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Auctions_AuctionId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AuctionWinners",
                newName: "WonAt");

            migrationBuilder.AddColumn<decimal>(
                name: "WinningBid",
                table: "AuctionWinners",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Auctions_AuctionId",
                table: "AnalyticsEvents",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Auctions_AuctionId",
                table: "Notifications",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Auctions_AuctionId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Auctions_AuctionId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "WinningBid",
                table: "AuctionWinners");

            migrationBuilder.RenameColumn(
                name: "WonAt",
                table: "AuctionWinners",
                newName: "CreatedAt");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Auctions_AuctionId",
                table: "AnalyticsEvents",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Auctions_AuctionId",
                table: "Notifications",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
