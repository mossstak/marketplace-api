using System;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Data;
using MarketPlaceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services
{
    public class ProductImagesService : IProductImagesService
    {
        private readonly ApplicationDbContext _db;

        public ProductImagesService(ApplicationDbContext db)
        {
            _db = db;
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

        public async Task<ProductImage> AttachSellerImageAsync(
            int productId,
            int sellerImageId,
            string? userId,
            bool isAdmin,
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

            var sellerImage = await _db.Set<SellerImage>()
                .FirstOrDefaultAsync(x => x.Id == sellerImageId && x.SellerId == product.SellerId);

            if (sellerImage == null)
                throw new KeyNotFoundException("Seller image not found.");

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
                ImageUrl = sellerImage.ImageUrl,
                PublicId = sellerImage.PublicId,
                IsPrimary = isPrimary,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Set<ProductImage>().Add(entity);
            await _db.SaveChangesAsync();

            return entity;
        }

        public async Task<ProductImage> SetPrimaryImageAsync(
            int productId,
            int imageId,
            string? userId,
            bool isAdmin
        )
        {
            if (string.IsNullOrWhiteSpace(userId) && !isAdmin)
                throw new UnauthorizedAccessException("User not authenticated.");

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            if (!isAdmin && product.SellerId != userId)
                throw new UnauthorizedAccessException("You don't own this product.");

            var image = await _db.Set<ProductImage>()
                .FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);

            if (image == null)
                throw new KeyNotFoundException("Image not found.");

            var existing = await _db.Set<ProductImage>()
                .Where(x => x.ProductId == productId && x.IsPrimary)
                .ToListAsync();

            foreach (var img in existing)
                img.IsPrimary = false;

            image.IsPrimary = true;
            await _db.SaveChangesAsync();

            return image;
        }

        public async Task DeleteImageAsync(
            int productId,
            int imageId,
            string? userId,
            bool isAdmin
        )
        {
            if (string.IsNullOrWhiteSpace(userId) && !isAdmin)
                throw new UnauthorizedAccessException("User not authenticated.");

            var product = await _db.Products.FindAsync(productId);
            if (product == null)
                throw new KeyNotFoundException("Product not found.");

            if (!isAdmin && product.SellerId != userId)
                throw new UnauthorizedAccessException("You don't own this product.");

            var image = await _db.Set<ProductImage>()
                .FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);

            if (image == null)
                throw new KeyNotFoundException("Image not found.");

            _db.Set<ProductImage>().Remove(image);
            await _db.SaveChangesAsync();
        }
    }
}
