using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketPlaceApi.Migrations
{
    /// <inheritdoc />
    public partial class moreupdateMade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Product_Name",
                table: "Products",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "Product_Description",
                table: "Products",
                newName: "ProductDescription");

            migrationBuilder.RenameColumn(
                name: "Postal_Code",
                table: "AspNetUsers",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "Last_Name",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "First_Name",
                table: "AspNetUsers",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Address_Two",
                table: "AspNetUsers",
                newName: "AddressTwo");

            migrationBuilder.RenameColumn(
                name: "Address_One",
                table: "AspNetUsers",
                newName: "AddressOne");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "Products",
                newName: "Product_Name");

            migrationBuilder.RenameColumn(
                name: "ProductDescription",
                table: "Products",
                newName: "Product_Description");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "AspNetUsers",
                newName: "Postal_Code");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "Last_Name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "AspNetUsers",
                newName: "First_Name");

            migrationBuilder.RenameColumn(
                name: "AddressTwo",
                table: "AspNetUsers",
                newName: "Address_Two");

            migrationBuilder.RenameColumn(
                name: "AddressOne",
                table: "AspNetUsers",
                newName: "Address_One");
        }
    }
}
