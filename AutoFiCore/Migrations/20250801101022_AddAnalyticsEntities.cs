using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    AuctionId = table.Column<int>(type: "integer", nullable: true),
                    EventData = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsEvents_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuctionAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuctionId = table.Column<int>(type: "integer", nullable: false),
                    TotalViews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UniqueBidders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalBids = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletionStatus = table.Column<bool>(type: "boolean", nullable: true),
                    ViewToBidRatio = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    StartPrice = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    FinalPrice = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    SuccessRate = table.Column<double>(type: "double precision", nullable: true),
                    EngagementScore = table.Column<double>(type: "double precision", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionAnalytics_Auctions_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auctions",
                        principalColumn: "AuctionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MetricType = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MetricType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ResponseTime = table.Column<int>(type: "integer", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_AuctionId",
                table: "AnalyticsEvents",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_CreatedAt",
                table: "AnalyticsEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType",
                table: "AnalyticsEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId",
                table: "AnalyticsEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionAnalytics_AuctionId",
                table: "AuctionAnalytics",
                column: "AuctionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuctionAnalytics_UpdatedAt",
                table: "AuctionAnalytics",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMetrics_Category",
                table: "DailyMetrics",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMetrics_Date_MetricType",
                table: "DailyMetrics",
                columns: new[] { "Date", "MetricType" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_CreatedAt",
                table: "PerformanceMetrics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_MetricType",
                table: "PerformanceMetrics",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_StatusCode",
                table: "PerformanceMetrics",
                column: "StatusCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

            migrationBuilder.DropTable(
                name: "AuctionAnalytics");

            migrationBuilder.DropTable(
                name: "DailyMetrics");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");
        }
    }
}
