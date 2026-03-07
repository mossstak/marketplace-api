using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCompanyNameFromUserAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company_Name",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Company_Name",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }
    }
}
