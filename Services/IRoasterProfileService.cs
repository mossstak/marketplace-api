using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface IRoasterProfileService
    {
        Task<RoasterProfileDto?> GetMyProfileAsync(string userId);
        Task<BecomeRoasterResponseDto> BecomeRoasterAsync(string userId, BecomeRoasterDto dto);
        Task<RoasterProfileDto> UpsertMyProfileAsync(string userId, UpsertRoasterProfileDto dto);
        Task<List<RoasterProfileDto>> GetAllRoaster();
        Task<List<RoasterProfileDto>> GetAllRoastersForAdminAsync();
        Task<RoasterProfileDto> GetPublicByUserIdAsync(string userId);
        Task<RoasterProfileDto> SetVerificationAsync(string userId, bool isVerified);
        Task<RoasterProfileDto> UpdateApprovalStatusAsync(string id, ApprovalStatus status);
    }
}
