using MarketPlaceApi.Data;
using MarketPlaceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services
{
    public class SellerImagesService : ISellerImagesService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICloudinarySigner _signer;

        public SellerImagesService(ApplicationDbContext db, ICloudinarySigner signer)
        {
            _db = db;
            _signer = signer;
        }

        public Task<object> CreateUploadSignatureAsync(string sellerId)
        {
            var folder = $"marketplace/sellers/{sellerId}";
            return Task.FromResult(_signer.CreateUploadSignature(folder));
        }

        public async Task<SellerImage> SaveSellerImageAsync(
            string sellerId,
            string imageUrl,
            string publicId
        )
        {
            var entity = new SellerImage
            {
                SellerId = sellerId,
                ImageUrl = imageUrl,
                PublicId = publicId,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Set<SellerImage>().Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<IReadOnlyList<SellerImage>> GetSellerImagesAsync(string sellerId)
        {
            var images = await _db.Set<SellerImage>()
                .Where(x => x.SellerId == sellerId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return images;
        }

        public async Task DeleteSellerImageAsync(string sellerId, int imageId)
        {
            var image = await _db.Set<SellerImage>()
                .FirstOrDefaultAsync(x => x.Id == imageId && x.SellerId == sellerId);

            if (image == null)
                throw new KeyNotFoundException("Image not found.");

            _db.Set<SellerImage>().Remove(image);
            await _db.SaveChangesAsync();
        }
    }
}
