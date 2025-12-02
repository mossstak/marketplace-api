using MarketPlaceApi.Models;

namespace MarketPlaceApi.Dtos
{
    public class EditProductDto
    {
        public string? Product_Name { get; set; }
        public string? Product_Description { get; set; }
        public ProductCategory? Category { get; set; }
        public int? RoastLevelId { get; set; }
        public int? CoffeeProcessId { get; set; }
        public int? RegionId { get; set; }
        public int? ProducerId { get; set; }
        public int? VarietalId { get; set; }
        public int? AltitudeId { get; set; }
        public string? TastingNotes { get; set; }
        public DateTime? RoastDate { get; set; }
        public List<UpdateVariantDto> Variants { get; set; } = new();

        public class EditVariantDto
        {
            public int? Id { get; set; }  // null = new variant
            public string Size { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}