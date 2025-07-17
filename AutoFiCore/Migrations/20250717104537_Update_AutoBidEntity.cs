using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Update_AutoBidEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoBids_BidStrategy_BidStrategyId",
                table: "AutoBids");

            migrationBuilder.DropIndex(
                name: "IX_AutoBids_BidStrategyId",
                table: "AutoBids");

            migrationBuilder.DropColumn(
                name: "BidStrategyId",
                table: "AutoBids");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BidStrategyId",
                table: "AutoBids",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AutoBids_BidStrategyId",
                table: "AutoBids",
                column: "BidStrategyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AutoBids_BidStrategy_BidStrategyId",
                table: "AutoBids",
                column: "BidStrategyId",
                principalTable: "BidStrategy",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
