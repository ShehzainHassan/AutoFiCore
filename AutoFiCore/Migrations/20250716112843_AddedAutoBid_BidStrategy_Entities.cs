using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedAutoBid_BidStrategy_Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "autoBids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AuctionId = table.Column<int>(type: "integer", nullable: false),
                    MaxBidAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentBidAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BidStrategyType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_autoBids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_autoBids_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_autoBids_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BidStrategy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    BidDelaySeconds = table.Column<int>(type: "integer", nullable: false),
                    MaxBidsPerMinute = table.Column<int>(type: "integer", nullable: false),
                    PreferredBidTiming = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulBids = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FailedBids = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidStrategy", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoBid_ActiveByAuction",
                table: "autoBids",
                columns: new[] { "AuctionId", "IsActive", "MaxBidAmount" });

            migrationBuilder.CreateIndex(
                name: "UX_AutoBid_User_Auction",
                table: "autoBids",
                columns: new[] { "UserId", "AuctionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BidStrategy_Timing",
                table: "BidStrategy",
                column: "PreferredBidTiming");

            migrationBuilder.CreateIndex(
                name: "IX_BidStrategy_Type",
                table: "BidStrategy",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "autoBids");

            migrationBuilder.DropTable(
                name: "BidStrategy");
        }
    }
}
