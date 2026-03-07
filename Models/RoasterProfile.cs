using Microsoft.AspNetCore.Identity;

namespace MarketPlaceApi.Models
{
    public class RoasterProfile
{
    public int Id { get; set; }

    // 1:1 with User
    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;

    // Public storefront fields
    public string? CompanyName { get; set; }
    public string? Bio { get; set; }

    // Location (public-friendly, NOT full address)
    public string? City { get; set; }
    public string? Country { get; set; }

    // Verified badge
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }

    // Socials (simple version: columns)
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TikTokUrl { get; set; }
    public string? XUrl { get; set; }
    public string? FacebookUrl { get; set; }
}

}