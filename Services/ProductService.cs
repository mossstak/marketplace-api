using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services
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

        //Creates Product
        public async Task<Product> CreateProductAsync(CreateProductDto dto, User seller)
        {
            // Block sellers from creating products until profile is complete
            var roasterProfile = await _context.Set<RoasterProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rp => rp.UserId == seller.Id);

            if (roasterProfile == null || string.IsNullOrWhiteSpace(roasterProfile.CompanyName))
            {
                throw new InvalidOperationException("Complete your roaster profile (company name) before creating products.");
            }

            var roastLevel = await _coffeeAttributeService.GetOrCreateRoastLevelAsync(dto.RoastLevelName);
            var process = await _coffeeAttributeService.GetOrCreateCoffeeProcessAsync(dto.CoffeeProcessName);
            var origin = await _coffeeAttributeService.GetOrCreateOriginAsync(dto.OriginName);
            var region = await _coffeeAttributeService.GetOrCreateRegionAsync(dto.RegionName);
            var producer = await _coffeeAttributeService.GetOrCreateProducerAsync(dto.ProducerName);
            var varietal = await _coffeeAttributeService.GetOrCreateVarietalAsync(dto.VarietalName);
            var altitude = await _coffeeAttributeService.GetOrCreateAltitudeAsync(dto.AltitudeValue);

            var product = new Product
            {
                ProductName = dto.ProductName,
                ProductDescription = dto.ProductDescription,
                Category = dto.Category,
                RoastLevelId = roastLevel.Id,
                CoffeeProcessId = process.Id,
                OriginId = origin.Id,
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

            if (dto.ImageIds != null && dto.ImageIds.Count > 0)
            {
                var imageIds = dto.ImageIds.Distinct().ToList();
                var sellerImages = await _context.Set<SellerImage>()
                    .Where(x => x.SellerId == seller.Id && imageIds.Contains(x.Id))
                    .ToListAsync();

                if (sellerImages.Count != imageIds.Count)
                    throw new InvalidOperationException("One or more images are not available.");

                var primaryId = dto.PrimaryImageId ?? imageIds.First();

                var productImages = sellerImages.Select(img => new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = img.ImageUrl,
                    PublicId = img.PublicId,
                    IsPrimary = img.Id == primaryId,
                    CreatedAtUtc = DateTime.UtcNow
                }).ToList();

                _context.Set<ProductImage>().AddRange(productImages);
                await _context.SaveChangesAsync();
            }

            return product;
        }

        //Edit Products
        public async Task EditProductAsync(int id, EditProductDto dto)
        {
            var product = await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException("Product Not Found");

            if (dto.ProductName != null)
                product.ProductName = dto.ProductName;
            if (dto.ProductDescription != null)
                product.ProductDescription = dto.ProductDescription;
            if (dto.Category.HasValue)
                product.Category = dto.Category.Value;
            if (dto.RoastLevelId.HasValue)
                product.RoastLevelId = dto.RoastLevelId.Value;
            if (dto.CoffeeProcessId.HasValue)
                product.CoffeeProcessId = dto.CoffeeProcessId.Value;
            if (dto.OriginId.HasValue)
                product.OriginId = dto.OriginId.Value;
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

        //Gets All Products
        public async Task<IEnumerable<object>> GetAllProductsAsync()
        {
            var products = await _context.Products.Select(p => new
            {
                p.Id,
                p.ProductName,
                p.ProductDescription,
                category = p.Category.ToString(),
                Seller = new { p.SellerId},
                Images = _context.ProductImages
                    .Where(img => img.ProductId == p.Id)
                    .OrderByDescending(img => img.IsPrimary)
                    .Select(img => new
                    {
                        img.Id,
                        img.ImageUrl,
                        img.IsPrimary
                    })
                    .ToList()
            }).ToListAsync();

            return products.Cast<object>();
        }

        //Get Product By Id
        public async Task<object> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.ProductDescription,
                    category = p.Category.ToString(),
                    Seller = new { p.SellerId },
                    Variants = p.Variants.Select(v => new
                    {
                        v.Id,
                        v.Size,
                        v.Price,
                        v.Quantity,
                    }),
                    Images = _context.ProductImages
                        .Where(img => img.ProductId == p.Id)
                        .OrderByDescending(img => img.IsPrimary)
                        .Select(img => new
                        {
                            img.Id,
                            img.ImageUrl,
                            img.IsPrimary
                        })
                        .ToList(),
                    roastLevel = p.RoastLevel.Name,
                    origin = p.Origin.Name,
                    producer = p.Producer.Name,
                    region = p.Region.Name,
                    coffeeProcess = p.CoffeeProcess.Name,
                    varietal = p.Varietal.Name,
                    altitude = p.Altitude.ValueInMasl,
                    p.TastingNotes,
                    p.RoastDate
                })
                .FirstOrDefaultAsync();

            if (product == null)
                throw new KeyNotFoundException("Product Not Found");

            return product;
        }

        //Gets Product by individual product via ID
        public async Task<IEnumerable<object>> GetProductsBySellerAsync(string sellerId)
        {
            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Select(p => new
                {
                    p.Id,
                    p.ProductName,
                    p.ProductDescription,
                    category = p.Category.ToString(),
                    Seller = new { p.SellerId },
                    Variants = p.Variants.Select(v => new
                    {
                        v.Id,
                        v.Size,
                        v.Price,
                        v.Quantity,
                    }),
                    Images = _context.ProductImages
                        .Where(img => img.ProductId == p.Id)
                        .OrderByDescending(img => img.IsPrimary)
                        .Select(img => new
                        {
                            img.Id,
                            img.ImageUrl,
                            img.IsPrimary
                        })
                        .ToList(),
                    roastLevel = p.RoastLevel.Name,
                    origin = p.Origin.Name,
                    producer = p.Producer.Name,
                    region = p.Region.Name,
                    coffeeProcess = p.CoffeeProcess.Name,
                    varietal = p.Varietal.Name,
                    altitude = p.Altitude.ValueInMasl,
                    p.TastingNotes,
                    p.RoastDate
                })
                .ToListAsync();

            return products.Cast<object>();
        }


        public async Task UpdateProductAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new KeyNotFoundException("Product Not Found");
            product.ProductName = dto.ProductName;
            product.ProductDescription = dto.ProductDescription;
            product.Category = dto.Category;
            product.RoastLevelId = dto.RoastLevelId;
            product.CoffeeProcessId = dto.CoffeeProcessId;
            product.OriginId = dto.OriginId;
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
