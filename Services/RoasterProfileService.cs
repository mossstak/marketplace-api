using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Services
{
    public class RoasterProfileService : IRoasterProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private readonly IStripeConnectService _stripeConnectService;
        private readonly ILogger<RoasterProfileService> _logger;

        public RoasterProfileService(
            ApplicationDbContext context,
            UserManager<User> userManager,
            TokenService tokenService,
            IStripeConnectService stripeConnectService,
            ILogger<RoasterProfileService> logger)
        {
            _context = context;
            _userManager = userManager;
            _tokenService = tokenService;
            _stripeConnectService = stripeConnectService;
            _logger = logger;
        }

        public async Task<RoasterProfileDto?> GetMyProfileAsync(string userId)
        {
            var profile = await _context.RoasterProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (profile == null)
            {
                return null;
            }

            return MapToDto(profile);
        }

        public async Task<BecomeRoasterResponseDto> BecomeRoasterAsync(string userId, BecomeRoasterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
            {
                throw new ValidationException("Company name is required.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var existingProfile = await _context.RoasterProfiles
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (existingProfile != null)
            {
                throw new InvalidOperationException("User already has a roaster profile.");
            }

            // Create RoasterProfile
            var profile = new RoasterProfile
            {
                UserId = userId,
                CompanyName = dto.CompanyName.Trim(),
                WebsiteUrl = string.IsNullOrWhiteSpace(dto.WebsiteUrl) ? null : dto.WebsiteUrl.Trim(),
                City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim(),
                Country = string.IsNullOrWhiteSpace(dto.Country) ? null : dto.Country.Trim(),
                IsVerified = false
            };

            _context.RoasterProfiles.Add(profile);

            // Update user address details if provided
            if (!string.IsNullOrWhiteSpace(dto.AddressOne)) user.AddressOne = dto.AddressOne.Trim();
            if (!string.IsNullOrWhiteSpace(dto.AddressTwo)) user.AddressTwo = dto.AddressTwo.Trim();
            if (!string.IsNullOrWhiteSpace(dto.City)) user.City = dto.City.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Country)) user.Country = dto.Country.Trim();
            if (!string.IsNullOrWhiteSpace(dto.PostalCode)) user.PostalCode = dto.PostalCode.Trim();

            await _userManager.UpdateAsync(user);

            // Assign "Seller" role to the user
            if (!await _userManager.IsInRoleAsync(user, "Seller"))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Seller");
                if (!roleResult.Succeeded)
                {
                    throw new Exception("Failed to assign Seller role to user.");
                }
            }

            await _context.SaveChangesAsync();

            // Refreshed token containing the "Seller" role
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            // Generate Stripe Connect onboarding link
            string? onboardingUrl = null;
            try
            {
                var onboardingDto = new CreateOnboardingLinkRequestDto
                {
                    RefreshUrl = string.IsNullOrWhiteSpace(dto.RefreshUrl) ? "http://localhost:3000/seller/dashboard/payouts" : dto.RefreshUrl,
                    ReturnUrl = string.IsNullOrWhiteSpace(dto.ReturnUrl) ? "http://localhost:3000/seller/dashboard/payouts" : dto.ReturnUrl,
                };
                var onboardingResult = await _stripeConnectService.CreateOrGetOnboardingLinkAsync(userId, onboardingDto);
                onboardingUrl = onboardingResult.OnboardingUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not automatically generate Stripe onboarding link during become-roaster for user {UserId}", userId);
            }

            return new BecomeRoasterResponseDto
            {
                Profile = MapToDto(profile),
                Token = token,
                Roles = roles,
                OnboardingUrl = onboardingUrl
            };
        }

        public async Task<RoasterProfileDto> UpsertMyProfileAsync(string userId, UpsertRoasterProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                throw new ValidationException("Company name is required for seller profiles.");

            var profile = await _context.RoasterProfiles
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (profile == null)
            {
                profile = new RoasterProfile
                {
                    UserId = userId,
                    IsVerified = false
                };
                _context.RoasterProfiles.Add(profile);
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
                .Where(rp => rp.ApprovalStatus == ApprovalStatus.Approved)
                .OrderBy(rp => rp.CompanyName)
                .Select(rp => new RoasterProfileDto
                {
                    Id = rp.Id,
                    UserId = rp.UserId,
                    CompanyName = rp.CompanyName,
                    Bio = rp.Bio,
                    City = rp.City,
                    Country = rp.Country,
                    WebsiteUrl = rp.WebsiteUrl,
                    InstagramUrl = rp.InstagramUrl,
                    IsVerified = rp.IsVerified,
                    ApprovalStatus = rp.ApprovalStatus
                })
                .ToListAsync();
        }

        public async Task<List<RoasterProfileDto>> GetAllRoastersForAdminAsync()
        {
            var profiles = await _context.RoasterProfiles
                .AsNoTracking()
                .OrderBy(rp => rp.CompanyName)
                .ToListAsync();

            return profiles.Select(MapToDto).ToList();
        }

        public async Task<RoasterProfileDto> GetPublicByUserIdAsync(string userId)
        {
            int? profileId = int.TryParse(userId, out var parsedId) ? parsedId : null;
            var profile = await _context.RoasterProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(rp =>
                    (rp.UserId == userId || (profileId.HasValue && rp.Id == profileId.Value))
                    && rp.ApprovalStatus == ApprovalStatus.Approved);

            if (profile == null)
                throw new KeyNotFoundException("Roaster profile not found.");

            return MapToDto(profile);
        }

        public async Task<RoasterProfileDto> SetVerificationAsync(string userId, bool isVerified)
        {
            var profile = await _context.RoasterProfiles
                .FirstOrDefaultAsync(rp => rp.UserId == userId);

            if (profile == null)
            {
                profile = new RoasterProfile
                {
                    UserId = userId
                };
                _context.RoasterProfiles.Add(profile);
            }

            profile.IsVerified = isVerified;
            profile.VerifiedAtUtc = isVerified ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();
            return MapToDto(profile);
        }

        public async Task<RoasterProfileDto> UpdateApprovalStatusAsync(string id, ApprovalStatus status)
        {
            int? profileId = int.TryParse(id, out var parsedId) ? parsedId : null;
            var profile = await _context.RoasterProfiles
                .FirstOrDefaultAsync(rp => (profileId.HasValue && rp.Id == profileId.Value) || rp.UserId == id);

            if (profile == null)
                throw new KeyNotFoundException($"Roaster profile '{id}' not found.");

            profile.ApprovalStatus = status;
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
                ApprovalStatus = profile.ApprovalStatus,
                WebsiteUrl = profile.WebsiteUrl,
                InstagramUrl = profile.InstagramUrl,
                TikTokUrl = profile.TikTokUrl,
                XUrl = profile.XUrl,
                FacebookUrl = profile.FacebookUrl
            };
        }
    }
}
