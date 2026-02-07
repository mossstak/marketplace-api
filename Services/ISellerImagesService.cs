using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface ISellerImagesService
    {
        Task<object> CreateUploadSignatureAsync(string sellerId);
        Task<SellerImage> SaveSellerImageAsync(string sellerId, string imageUrl, string publicId);
        Task<IReadOnlyList<SellerImage>> GetSellerImagesAsync(string sellerId);
        Task DeleteSellerImageAsync(string sellerId, int imageId);
    }
}
