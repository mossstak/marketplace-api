using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoffeeAltitude",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ValueInMasl = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeAltitude", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeProcess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeProcess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeProducer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeProducer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeRegion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeRegion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeVarietal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeVarietal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoastLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoastLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Product_Name = table.Column<string>(type: "text", nullable: true),
                    Product_Description = table.Column<string>(type: "text", nullable: true),
                    Product_Price = table.Column<float>(type: "real", nullable: false),
                    RoastLevelId = table.Column<int>(type: "integer", nullable: false),
                    CoffeeProcessId = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false),
                    ProducerId = table.Column<int>(type: "integer", nullable: false),
                    VarietalId = table.Column<int>(type: "integer", nullable: false),
                    AltitudeId = table.Column<int>(type: "integer", nullable: false),
                    TastingNotes = table.Column<string>(type: "text", nullable: true),
                    RoastDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_CoffeeAltitude_AltitudeId",
                        column: x => x.AltitudeId,
                        principalTable: "CoffeeAltitude",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_CoffeeProcess_CoffeeProcessId",
                        column: x => x.CoffeeProcessId,
                        principalTable: "CoffeeProcess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_CoffeeProducer_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "CoffeeProducer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_CoffeeRegion_RegionId",
                        column: x => x.RegionId,
                        principalTable: "CoffeeRegion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_CoffeeVarietal_VarietalId",
                        column: x => x.VarietalId,
                        principalTable: "CoffeeVarietal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_RoastLevel_RoastLevelId",
                        column: x => x.RoastLevelId,
                        principalTable: "RoastLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product_AltitudeId",
                table: "Product",
                column: "AltitudeId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CoffeeProcessId",
                table: "Product",
                column: "CoffeeProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProducerId",
                table: "Product",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_RegionId",
                table: "Product",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_RoastLevelId",
                table: "Product",
                column: "RoastLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_VarietalId",
                table: "Product",
                column: "VarietalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "CoffeeAltitude");

            migrationBuilder.DropTable(
                name: "CoffeeProcess");

            migrationBuilder.DropTable(
                name: "CoffeeProducer");

            migrationBuilder.DropTable(
                name: "CoffeeRegion");

            migrationBuilder.DropTable(
                name: "CoffeeVarietal");

            migrationBuilder.DropTable(
                name: "RoastLevel");
        }
    }
}
