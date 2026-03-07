using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MarketPlaceApi.Services
{
    public class RoasterProfileService : IRoasterProfileService
    {
        private readonly ApplicationDbContext _context;

        public RoasterProfileService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RoasterProfileDto> GetMyProfileAsync(string userId)
        {
            var profile = await _context.Set<RoasterProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            // If you prefer: auto-create an empty profile the first time someone visits.
            if (profile == null)
            {
                profile = new RoasterProfile
                {
                    UserId = userId,
                    IsVerified = false
                };

                _context.Set<RoasterProfile>().Add(profile);
                await _context.SaveChangesAsync();
            }

            return MapToDto(profile);
        }

        public async Task<RoasterProfileDto> UpsertMyProfileAsync(string userId, UpsertRoasterProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                throw new ValidationException("Company name is required for seller profiles.");

            var profile = await _context.Set<RoasterProfile>()
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (profile == null)
            {
                profile = new RoasterProfile
                {
                    UserId = userId,
                    IsVerified = false
                };
                _context.Set<RoasterProfile>().Add(profile);
            }

            // Upsert fields (only overwrite when provided)
            if (dto.CompanyName != null) profile.CompanyName = dto.CompanyName;

            if (dto.Bio != null) profile.Bio = dto.Bio;
            if (dto.City != null) profile.City = dto.City;
            if (dto.Country != null) profile.Country = dto.Country;

            if (dto.WebsiteUrl != null) profile.WebsiteUrl = dto.WebsiteUrl;
            if (dto.InstagramUrl != null) profile.InstagramUrl = dto.InstagramUrl;
            if (dto.TikTokUrl != null) profile.TikTokUrl = dto.TikTokUrl;
            if (dto.XUrl != null) profile.XUrl = dto.XUrl;
            if (dto.FacebookUrl != null) profile.FacebookUrl = dto.FacebookUrl;

            await _context.SaveChangesAsync();
            return MapToDto(profile);
        }

        public async Task<List<RoasterProfileDto>> GetAllRoaster()
        {
            return await _context.RoasterProfiles
                .AsNoTracking()
                .OrderBy(rp => rp.CompanyName)
                .Select(rp => new RoasterProfileDto
                {
                    UserId = rp.UserId,
                    CompanyName = rp.CompanyName,
                    Bio = rp.Bio,
                    City = rp.City,
                    Country = rp.Country,
                    WebsiteUrl = rp.WebsiteUrl,
                    InstagramUrl = rp.InstagramUrl,
                    IsVerified = rp.IsVerified
                })
                .ToListAsync();
        }

        public async Task<RoasterProfileDto> GetPublicByUserIdAsync(string userId)
        {
            var profile = await _context.Set<RoasterProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (profile == null)
                throw new KeyNotFoundException("Roaster profile not found.");

            return MapToDto(profile);
        }

        public async Task<RoasterProfileDto> SetVerificationAsync(string userId, bool isVerified)
        {
            var profile = await _context.Set<RoasterProfile>()
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (profile == null)
            {
                profile = new RoasterProfile
                {
                    UserId = userId
                };
                _context.Set<RoasterProfile>().Add(profile);
            }

            profile.IsVerified = isVerified;
            profile.VerifiedAtUtc = isVerified ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();
            return MapToDto(profile);
        }

        private static RoasterProfileDto MapToDto(RoasterProfile profile)
        {
            return new RoasterProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                CompanyName = profile.CompanyName,
                Bio = profile.Bio,
                City = profile.City,
                Country = profile.Country,
                IsVerified = profile.IsVerified,
                VerifiedAtUtc = profile.VerifiedAtUtc,
                WebsiteUrl = profile.WebsiteUrl,
                InstagramUrl = profile.InstagramUrl,
                TikTokUrl = profile.TikTokUrl,
                XUrl = profile.XUrl,
                FacebookUrl = profile.FacebookUrl
            };
        }
    }
}
