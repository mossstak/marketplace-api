using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Controllers
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICoffeeAttributeService _coffeeAttributeService;

        public ProductService(
            ApplicationDbContext context,
            ICoffeeAttributeService coffeeAttributeService
        )
        {
            _context = context;
            _coffeeAttributeService = coffeeAttributeService;
        }
        public async Task<Product> CreateProductAsync(CreateProductDto dto, User seller)
        {
            var roastLevel = await _coffeeAttributeService.GetOrCreateRoastLevelAsync(dto.RoastLevelName);
            var process = await _coffeeAttributeService.GetOrCreateCoffeeProcessAsync(dto.CoffeeProcessName);
            var region = await _coffeeAttributeService.GetOrCreateRegionAsync(dto.RegionName);
            var producer = await _coffeeAttributeService.GetOrCreateProducerAsync(dto.ProducerName);
            var varietal = await _coffeeAttributeService.GetOrCreateVarietalAsync(dto.VarietalName);
            var altitude = await _coffeeAttributeService.GetOrCreateAltitudeAsync(dto.AltitudeValue);

            var product = new Product
            {
                Product_Name = dto.Product_Name,
                Product_Description = dto.Product_Description,
                Category = dto.Category,
                RoastLevelId = roastLevel.Id,
                CoffeeProcessId = process.Id,
                RegionId = region.Id,
                ProducerId = producer.Id,
                VarietalId = varietal.Id,
                AltitudeId = altitude.Id,

                TastingNotes = dto.TastingNotes,
                RoastDate = dto.RoastDate,
                SellerId = seller.Id,

                Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Price = v.Price,
                    Quantity = v.Quantity
                }).ToList()
            };

            if (dto.Variants.Count > 6)
            {
                throw new InvalidOperationException("Product can only have 6 variants per item.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task EditProductAsync(int id, EditProductDto dto)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException("Product Not Found");

            if (dto.Product_Name != null)
                product.Product_Name = dto.Product_Name;
            if (dto.Product_Description != null)
                product.Product_Description = dto.Product_Description;
            if (dto.Category.HasValue)
                product.Category = dto.Category.Value;
            if (dto.RoastLevelId.HasValue)
                product.RoastLevelId = dto.RoastLevelId.Value;
            if (dto.CoffeeProcessId.HasValue)
                product.CoffeeProcessId = dto.CoffeeProcessId.Value;
            if (dto.RegionId.HasValue)
                product.RegionId = dto.RegionId.Value;
            if (dto.ProducerId.HasValue)
                product.ProducerId = dto.ProducerId.Value;
            if (dto.VarietalId.HasValue)
                product.VarietalId = dto.VarietalId.Value;
            if (dto.AltitudeId.HasValue)
                product.AltitudeId = dto.AltitudeId.Value;
            if (dto.TastingNotes != null)
                product.TastingNotes = dto.TastingNotes;
            if (dto.RoastDate.HasValue)
                product.RoastDate = dto.RoastDate.Value;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<object>> GetAllProductsAsync()
        {
            var products = await _context.Products.Select(p => new
            {
                p.Id,
                p.Product_Name,
                p.Product_Description,
                category = p.Category.ToString(),
                Seller = new { p.SellerId, p.Seller!.Email, p.Seller.Company_Name },
                Variants = p.Variants.Select(v => new
                {
                    v.Id,
                    v.Size,
                    v.Price,
                    v.Quantity,
                }),
                producer = p.Producer.Name,
                region = p.Region.Name,
                coffeeprocess = p.CoffeeProcess.Name,
                varietal = p.Varietal.Name,
                altitude = p.Altitude.ValueInMasl,
                p.TastingNotes,
                p.RoastDate
            }).ToListAsync();

            return products.Cast<object>();
        }

        public async Task UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new KeyNotFoundException("Product Not Found");
            product.Product_Name = dto.Product_Name;
            product.Product_Description = dto.Product_Description;
            product.Category = dto.Category;
            product.RoastLevelId = dto.RoastLevelId;
            product.CoffeeProcessId = dto.CoffeeProcessId;
            product.RegionId = dto.RegionId;
            product.ProducerId = dto.ProducerId;
            product.VarietalId = dto.VarietalId;
            product.AltitudeId = dto.AltitudeId;
            product.TastingNotes = dto.TastingNotes;
            product.RoastDate = dto.RoastDate;

            product.Variants.Clear();

            foreach (var v in dto.Variants)
            {
                product.Variants.Add(new ProductVariant
                {
                    Id = v.Id ?? 0,
                    Size = v.Size,
                    Price = v.Price,
                    Quantity = v.Quantity,
                    ProductId = product.Id
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException("Product Not Found");

            _context.ProductVariants.RemoveRange(product.Variants);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

    }
}