using System.ComponentModel.DataAnnotations;

namespace MarketPlaceApi.Dtos
{
    public class BecomeRoasterDto
    {
        [Required(ErrorMessage = "Company name is required.")]
        public string CompanyName { get; set; } = default!;

        public string? WebsiteUrl { get; set; }

        public string? AddressOne { get; set; }
        public string? AddressTwo { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public string? RefreshUrl { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class BecomeRoasterResponseDto
    {
        public RoasterProfileDto Profile { get; set; } = default!;
        public string Token { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public string? OnboardingUrl { get; set; }
    }
}
