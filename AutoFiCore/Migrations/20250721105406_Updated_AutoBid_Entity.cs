using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_AutoBid_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_AutoBid_User_Auction",
                table: "AutoBids");

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_UserId",
                table: "AutoBids",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AutoBids_UserId",
                table: "AutoBids");

            migrationBuilder.CreateIndex(
                name: "UX_AutoBid_User_Auction",
                table: "AutoBids",
                columns: new[] { "UserId", "AuctionId" },
                unique: true);
        }
    }
}
