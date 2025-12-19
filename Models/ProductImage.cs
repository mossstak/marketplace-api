using MarketPlaceApi.Models;

namespace MarketPlaceApi.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = default!;
        public string PublicId { get; set; } = default!;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Product Product { get; set; } = default!;
    }
}
