using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Add_BidStrategyAuction_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BidStrategy_AuctionId",
                table: "BidStrategy",
                column: "AuctionId");

            migrationBuilder.AddForeignKey(
                name: "FK_BidStrategy_Auctions_AuctionId",
                table: "BidStrategy",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BidStrategy_Auctions_AuctionId",
                table: "BidStrategy");

            migrationBuilder.DropIndex(
                name: "IX_BidStrategy_AuctionId",
                table: "BidStrategy");
        }
    }
}
