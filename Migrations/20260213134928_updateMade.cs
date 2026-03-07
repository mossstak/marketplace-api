using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class updateMade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Company_Name",
                table: "RoasterProfiles",
                newName: "CompanyName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "RoasterProfiles",
                newName: "Company_Name");
        }
    }
}
