using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface IProductImagesService
    {
        Task<ProductImage> CreateUploadSignatureAsync(int productId, string? userId, bool isAdmin);

        Task<ProductImage> SaveProductImageAsync(
            int productId,
            string? userId,
            bool isAdmin,
            string imageUrl,
            string publicId,
            bool isPrimary
        );
    }
}