using MarketPlaceApi.Models;

namespace MarketPlaceApi.Models
{
    public class SellerImage
    {
        public int Id { get; set; }
        public string SellerId { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public string PublicId { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
