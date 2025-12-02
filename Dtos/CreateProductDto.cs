using MarketPlaceApi.Models;

namespace MarketPlaceApi.Dtos
{
    public class CreateProductDto
    {
        public string? Product_Name { get; set; }
        public string? Product_Description { get; set; } = "";
        public ProductCategory Category {get; set;}
        public string RoastLevelName { get; set; } = "";
        public string CoffeeProcessName { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string ProducerName { get; set; } = "";
        public string VarietalName { get; set; } = "";
        public double AltitudeValue { get; set; }
        public string TastingNotes { get; set; } = "";
        public DateTime RoastDate { get; set; }
        public List<CreateVariantDto> Variants { get; set; } = new List<CreateVariantDto>();
    }

    public class CreateVariantDto
    {
        public string Size { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}