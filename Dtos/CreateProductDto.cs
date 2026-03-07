using MarketPlaceApi.Models;

namespace MarketPlaceApi.Dtos
{
    public class CreateProductDto
    {
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; } = "";
        public ProductCategory Category {get; set;}
        public string RoastLevelName { get; set; } = "";
        public string CoffeeProcessName { get; set; } = "";
        public string OriginName { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string ProducerName { get; set; } = "";
        public string VarietalName { get; set; } = "";
        public double AltitudeValue { get; set; }
        public string TastingNotes { get; set; } = "";
        public DateTime RoastDate { get; set; }
        public List<CreateVariantDto> Variants { get; set; } = new List<CreateVariantDto>();
        public List<int> ImageIds { get; set; } = new List<int>();
        public int? PrimaryImageId { get; set; }
    }

    public class CreateVariantDto
    {
        public string Size { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
