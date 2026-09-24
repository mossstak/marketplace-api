using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly IEmailSender _emailSender;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public async Task<User> Register(RegisterDto dto)
        {
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Buyer" : dto.Role;
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new Exception("User creation failed.");

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                throw new Exception("Failed to assign role.");

            return user;
        }

        public async Task<User> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid credentials.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!check.Succeeded)
                throw new Exception("Invalid credentials.");

            return user;
        }

        public async Task<IEnumerable<object>> GetAllUser()
        {
            var users = await _userManager.Users.ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    Roles = roles
                });
            }

            return result;
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _userManager.Users
                .Include(u => u.RoasterProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.ToString()) ?? throw new KeyNotFoundException("User Not Found");
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;

            user.AddressOne = dto.AddressOne;
            user.AddressTwo = dto.AddressTwo;
            user.City = dto.City;
            user.Country = dto.Country;
            user.PostalCode = dto.PostalCode;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception("Failed to update user");
        }

        public async Task EditUserAsync(string id, EditUserDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.RoasterProfile)
                .FirstOrDefaultAsync(u => u.Id == id.ToString()) ?? throw new KeyNotFoundException("User Not Found");

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;

            if (dto.AddressOne != null)
                user.AddressOne = dto.AddressOne;

            if (dto.AddressTwo != null)
                user.AddressTwo = dto.AddressTwo;

            if (dto.City != null)
                user.City = dto.City;

            if (dto.Country != null)
                user.Country = dto.Country;

            if (dto.PostalCode != null)
                user.PostalCode = dto.PostalCode;

            if (dto.ProfileImageUrl != null)
                user.ProfileImageUrl = dto.ProfileImageUrl;

            if (!string.IsNullOrWhiteSpace(dto.Company_Name) && user.RoasterProfile != null)
            {
                user.RoasterProfile.CompanyName = dto.Company_Name;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception("Failed to update user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");
            var result = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!result.Succeeded)
                throw new Exception(
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
        }

        public async Task ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
                throw new Exception(
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            // Security practice: do not reveal whether the email exists
            if (user == null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Build the frontend URL with URL-encoded parameters
            var frontendBaseUrl = "http://localhost:3000"; // Or read from _config["Frontend:BaseUrl"]
            var resetLink = $"{frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Reset Your Marketplace Password",
                $"<p>Click <a href='{resetLink}'>here</a> to reset your password. This link is valid for 1 day.</p>"
            );
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                throw new InvalidOperationException("Invalid password reset request.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }

    }
}
