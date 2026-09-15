using System.ComponentModel.DataAnnotations;

namespace MarketPlaceApi.Dtos
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        public string Role { get; set; } = "Buyer";
    }
}