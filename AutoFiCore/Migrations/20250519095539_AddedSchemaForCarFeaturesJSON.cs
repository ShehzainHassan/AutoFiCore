using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoFiCore.Migrations
{
    /// <inheritdoc />
    public partial class AddedSchemaForCarFeaturesJSON : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivetrains",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Transmission = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivetrains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivetrains_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Engines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    Size = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    Horsepower = table.Column<int>(type: "integer", nullable: false),
                    TorqueFtLBS = table.Column<int>(type: "integer", nullable: false),
                    TorqueRPM = table.Column<int>(type: "integer", nullable: true),
                    Valves = table.Column<int>(type: "integer", nullable: true),
                    CamType = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Engines_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FuelEconomies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    FuelTankSize = table.Column<int>(type: "integer", nullable: true),
                    CombinedMPG = table.Column<int>(type: "integer", nullable: false),
                    CityMPG = table.Column<int>(type: "integer", nullable: false),
                    HighwayMPG = table.Column<int>(type: "integer", nullable: false),
                    CO2Emissions = table.Column<int>(type: "integer", nullable: false),
                    BetterCapacityKWH = table.Column<int>(type: "integer", nullable: false),
                    RangeMiles = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelEconomies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelEconomies_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    Doors = table.Column<int>(type: "integer", nullable: false),
                    MaximumSeating = table.Column<int>(type: "integer", nullable: false),
                    HeightInches = table.Column<int>(type: "integer", nullable: false),
                    WidthInches = table.Column<int>(type: "integer", nullable: false),
                    LengthInches = table.Column<int>(type: "integer", nullable: false),
                    WheelbaseInches = table.Column<int>(type: "integer", nullable: false),
                    GroundClearance = table.Column<int>(type: "integer", nullable: false),
                    CargoCapacityCuFt = table.Column<int>(type: "integer", nullable: true),
                    CargoWeightLBS = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    Options = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehiclePerformances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    ZeroTo60MPH = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehiclePerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiclePerformances_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle_VehicleOptions_Mapping",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "integer", nullable: false),
                    VehicleOptionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle_VehicleOptions_Mapping", x => new { x.VehicleId, x.VehicleOptionsId });
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleOptions_Mapping_VehicleOptions_VehicleOption~",
                        column: x => x.VehicleOptionsId,
                        principalTable: "VehicleOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleOptions_Mapping_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drivetrains_VehicleId",
                table: "Drivetrains",
                column: "VehicleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engines_VehicleId",
                table: "Engines",
                column: "VehicleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuelEconomies_VehicleId",
                table: "FuelEconomies",
                column: "VehicleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_VehicleId",
                table: "Measurements",
                column: "VehicleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_VehicleOptions_Mapping_VehicleOptionsId",
                table: "Vehicle_VehicleOptions_Mapping",
                column: "VehicleOptionsId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePerformances_VehicleId",
                table: "VehiclePerformances",
                column: "VehicleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drivetrains");

            migrationBuilder.DropTable(
                name: "Engines");

            migrationBuilder.DropTable(
                name: "FuelEconomies");

            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "Vehicle_VehicleOptions_Mapping");

            migrationBuilder.DropTable(
                name: "VehiclePerformances");

            migrationBuilder.DropTable(
                name: "VehicleOptions");
        }
    }
}
