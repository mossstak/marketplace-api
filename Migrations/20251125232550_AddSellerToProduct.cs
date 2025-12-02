using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_CoffeeAltitude_AltitudeId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_CoffeeProcess_CoffeeProcessId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_CoffeeProducer_ProducerId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_CoffeeRegion_RegionId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_CoffeeVarietal_VarietalId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_RoastLevel_RoastLevelId",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameIndex(
                name: "IX_Product_VarietalId",
                table: "Products",
                newName: "IX_Products_VarietalId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_RoastLevelId",
                table: "Products",
                newName: "IX_Products_RoastLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_RegionId",
                table: "Products",
                newName: "IX_Products_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_ProducerId",
                table: "Products",
                newName: "IX_Products_ProducerId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_CoffeeProcessId",
                table: "Products",
                newName: "IX_Products_CoffeeProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_AltitudeId",
                table: "Products",
                newName: "IX_Products_AltitudeId");

            migrationBuilder.AddColumn<string>(
                name: "SellerId",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SellerId",
                table: "Products",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_AspNetUsers_SellerId",
                table: "Products",
                column: "SellerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CoffeeAltitude_AltitudeId",
                table: "Products",
                column: "AltitudeId",
                principalTable: "CoffeeAltitude",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CoffeeProcess_CoffeeProcessId",
                table: "Products",
                column: "CoffeeProcessId",
                principalTable: "CoffeeProcess",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CoffeeProducer_ProducerId",
                table: "Products",
                column: "ProducerId",
                principalTable: "CoffeeProducer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CoffeeRegion_RegionId",
                table: "Products",
                column: "RegionId",
                principalTable: "CoffeeRegion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CoffeeVarietal_VarietalId",
                table: "Products",
                column: "VarietalId",
                principalTable: "CoffeeVarietal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_RoastLevel_RoastLevelId",
                table: "Products",
                column: "RoastLevelId",
                principalTable: "RoastLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_AspNetUsers_SellerId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CoffeeAltitude_AltitudeId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CoffeeProcess_CoffeeProcessId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CoffeeProducer_ProducerId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CoffeeRegion_RegionId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_CoffeeVarietal_VarietalId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_RoastLevel_RoastLevelId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SellerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Products");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameIndex(
                name: "IX_Products_VarietalId",
                table: "Product",
                newName: "IX_Product_VarietalId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_RoastLevelId",
                table: "Product",
                newName: "IX_Product_RoastLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_RegionId",
                table: "Product",
                newName: "IX_Product_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_ProducerId",
                table: "Product",
                newName: "IX_Product_ProducerId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CoffeeProcessId",
                table: "Product",
                newName: "IX_Product_CoffeeProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_AltitudeId",
                table: "Product",
                newName: "IX_Product_AltitudeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_CoffeeAltitude_AltitudeId",
                table: "Product",
                column: "AltitudeId",
                principalTable: "CoffeeAltitude",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_CoffeeProcess_CoffeeProcessId",
                table: "Product",
                column: "CoffeeProcessId",
                principalTable: "CoffeeProcess",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_CoffeeProducer_ProducerId",
                table: "Product",
                column: "ProducerId",
                principalTable: "CoffeeProducer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_CoffeeRegion_RegionId",
                table: "Product",
                column: "RegionId",
                principalTable: "CoffeeRegion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_CoffeeVarietal_VarietalId",
                table: "Product",
                column: "VarietalId",
                principalTable: "CoffeeVarietal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_RoastLevel_RoastLevelId",
                table: "Product",
                column: "RoastLevelId",
                principalTable: "RoastLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
