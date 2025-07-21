using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class Updated_BidStrategy_Table_and_Relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BidStrategies",
                table: "BidStrategies");

            migrationBuilder.DropIndex(
                name: "IX_BidStrategies_AuctionId",
                table: "BidStrategies");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BidStrategies",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "BidStrategies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BidStrategies",
                table: "BidStrategies",
                columns: new[] { "AuctionId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BidStrategies_UserId",
                table: "BidStrategies",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BidStrategies_Users_UserId",
                table: "BidStrategies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BidStrategies_Users_UserId",
                table: "BidStrategies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BidStrategies",
                table: "BidStrategies");

            migrationBuilder.DropIndex(
                name: "IX_BidStrategies_UserId",
                table: "BidStrategies");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "BidStrategies",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "BidStrategies",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BidStrategies",
                table: "BidStrategies",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BidStrategies_AuctionId",
                table: "BidStrategies",
                column: "AuctionId");
        }
    }
}
