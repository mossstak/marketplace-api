using System.ComponentModel.DataAnnotations;

namespace MarketPlaceApi.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string First_Name { get; set; } = default!;

        [Required]
        public string Last_Name { get; set; } = default!;

        [Required]
        public string Role { get; set; } = default!;   // "Seller", "Buyer", "Admin"

        // Optional-ish fields, enforced in controller based on Role
        public string Company_Name { get; set; } = "";

        public string Address_One { get; set; } = "";
        public string Address_Two { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Postal_Code { get; set; } = "";
    }
}