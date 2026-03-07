using MarketPlaceApi.Dtos;

namespace MarketPlaceApi.Services
{
    public interface IRoasterProfileService
    {
        Task<RoasterProfileDto> GetMyProfileAsync(string userId);
        Task<RoasterProfileDto> UpsertMyProfileAsync(string userId, UpsertRoasterProfileDto dto);
        Task<List<RoasterProfileDto>> GetAllRoaster();
        Task<RoasterProfileDto> GetPublicByUserIdAsync(string userId);
        Task<RoasterProfileDto> SetVerificationAsync(string userId, bool isVerified);
    }
}
