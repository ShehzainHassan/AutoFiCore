using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Renamed_Db_Set_AutoBids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_autoBids_Auctions_AuctionId",
                table: "autoBids");

            migrationBuilder.DropForeignKey(
                name: "FK_autoBids_Users_UserId",
                table: "autoBids");

            migrationBuilder.DropPrimaryKey(
                name: "PK_autoBids",
                table: "autoBids");

            migrationBuilder.RenameTable(
                name: "autoBids",
                newName: "AutoBids");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AutoBids",
                table: "AutoBids",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AutoBids_Auctions_AuctionId",
                table: "AutoBids",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AutoBids_Users_UserId",
                table: "AutoBids",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AutoBids_Auctions_AuctionId",
                table: "AutoBids");

            migrationBuilder.DropForeignKey(
                name: "FK_AutoBids_Users_UserId",
                table: "AutoBids");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AutoBids",
                table: "AutoBids");

            migrationBuilder.RenameTable(
                name: "AutoBids",
                newName: "autoBids");

            migrationBuilder.AddPrimaryKey(
                name: "PK_autoBids",
                table: "autoBids",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_autoBids_Auctions_AuctionId",
                table: "autoBids",
                column: "AuctionId",
                principalTable: "Auctions",
                principalColumn: "AuctionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_autoBids_Users_UserId",
                table: "autoBids",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
