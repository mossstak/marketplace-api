namespace MarketPlaceApi.Dtos
{
    public class UpsertRoasterProfileDto
    {
        // Public storefront fields
        public string? CompanyName { get; set; }
        public string? Bio { get; set; }

        // Location (public-friendly, NOT full address)
        public string? City { get; set; }
        public string? Country { get; set; }

        // Socials
        public string? WebsiteUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TikTokUrl { get; set; }
        public string? XUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
}
