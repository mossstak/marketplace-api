using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Models
{
    public class RoastLevel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class CoffeeProcess
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class CoffeeRegion
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

    }
    public class CoffeeProducer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class CoffeeVarietal
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
    public class CoffeeAltitude
    {
        public int Id { get; set; }
        public double ValueInMasl { get; set; }
    }

    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string? Size { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
    public class Product
    {

        // Product Main
        public int Id { get; set; }
        public string Product_Name { get; set; } = "";
        public string? Product_Description { get; set; } = "";

        // Product Category
        public ProductCategory Category {get; set;}

        // Coffee Metadata
        public int RoastLevelId { get; set; }
        public RoastLevel RoastLevel { get; set; }
        public int CoffeeProcessId { get; set; }
        public CoffeeProcess CoffeeProcess { get; set; }
        public int RegionId { get; set; }
        public CoffeeRegion Region { get; set; }
        public int ProducerId { get; set; }
        public CoffeeProducer Producer { get; set; }
        public int VarietalId { get; set; }
        public CoffeeVarietal Varietal { get; set; }
        public int AltitudeId { get; set; }
        public CoffeeAltitude Altitude { get; set; }
        public string? TastingNotes { get; set; }
        public DateTime RoastDate { get; set; }


        //One Seller has Multiple Products
        public string? SellerId { get; set; }
        public User? Seller { get; set; }

        //Product Variants
        public List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();


    }
}