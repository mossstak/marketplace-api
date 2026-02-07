using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface IProductImagesService
    {
        Task<ProductImage> SaveProductImageAsync(
            int productId,
            string? userId,
            bool isAdmin,
            string imageUrl,
            string publicId,
            bool isPrimary
        );

        Task<ProductImage> AttachSellerImageAsync(
            int productId,
            int sellerImageId,
            string? userId,
            bool isAdmin,
            bool isPrimary
        );

        Task<ProductImage> SetPrimaryImageAsync(int productId, int imageId, string? userId, bool isAdmin);

        Task DeleteImageAsync(int productId, int imageId, string? userId, bool isAdmin);
    }
}
