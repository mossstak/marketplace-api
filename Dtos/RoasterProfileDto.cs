namespace MarketPlaceApi.Dtos
{
    public class RoasterProfileDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;

        public string? CompanyName { get; set; }
        public string? Bio { get; set; }

        public string? City { get; set; }
        public string? Country { get; set; }

        public bool IsVerified { get; set; }
        public DateTime? VerifiedAtUtc { get; set; }

        public string? WebsiteUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TikTokUrl { get; set; }
        public string? XUrl { get; set; }
        public string? FacebookUrl { get; set; }
    }
}
