using System;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Data;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services; // ICloudinarySigner
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services
{
    public class ProductImagesService : IProductImagesService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICloudinarySigner _signer;

        public ProductImagesService(ApplicationDbContext db, ICloudinarySigner signer)
        {
            _db = db;
            _signer = signer;
        }

        public async Task<ProductImage> CreateUploadSignatureAsync(int productId, string? userId, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(userId) && !isAdmin)
                throw new UnauthorizedAccessException("User not authenticated");

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product Not Found");

            // IMPORTANT: adjust this if your Product uses a different field name/type
            if (!isAdmin && product.SellerId != userId)
                throw new UnauthorizedAccessException("You don't own this product.");

            var folder = $"marketplace/products/{productId}";
            return (ProductImage)_signer.CreateUploadSignature(folder);
        }

        public async Task<ProductImage> SaveProductImageAsync(
            int productId,
            string? userId,
            bool isAdmin,
            string imageUrl,
            string publicId,
            bool isPrimary
        )
        {
            if (string.IsNullOrWhiteSpace(userId) && !isAdmin)
                throw new UnauthorizedAccessException("User not authenticated.");

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            if (!isAdmin && product.SellerId != userId)
                throw new UnauthorizedAccessException("You don't own this product.");

            // If setting primary, unset other primaries for this product
            if (isPrimary)
            {
                var existing = await _db.Set<ProductImage>()
                    .Where(x => x.ProductId == productId && x.IsPrimary)
                    .ToListAsync();

                foreach (var img in existing)
                    img.IsPrimary = false;
            }

            var entity = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                PublicId = publicId,
                IsPrimary = isPrimary,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Set<ProductImage>().Add(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}