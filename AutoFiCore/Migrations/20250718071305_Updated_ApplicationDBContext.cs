using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_ApplicationDBContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BidStrategy_Auctions_AuctionId",
                table: "BidStrategy");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BidStrategy",
                table: "BidStrategy");

            migrationBuilder.RenameTable(
                name: "BidStrategy",
                newName: "BidStrategies");

            migrationBuilder.RenameIndex(
                name: "IX_BidStrategy_AuctionId",
                table: "BidStrategies",
                newName: "IX_BidStrategies_AuctionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BidStrategies",
                table: "BidStrategies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BidStrategies_Auctions_AuctionId",
                table: "BidStrategies",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BidStrategies_Auctions_AuctionId",
                table: "BidStrategies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BidStrategies",
                table: "BidStrategies");

            migrationBuilder.RenameTable(
                name: "BidStrategies",
                newName: "BidStrategy");

            migrationBuilder.RenameIndex(
                name: "IX_BidStrategies_AuctionId",
                table: "BidStrategy",
                newName: "IX_BidStrategy_AuctionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BidStrategy",
                table: "BidStrategy",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BidStrategy_Auctions_AuctionId",
                table: "BidStrategy",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
