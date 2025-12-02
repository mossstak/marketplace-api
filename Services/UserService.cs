using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<User> Register(RegisterDto dto)
        {
            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                First_Name = dto.First_Name,
                Last_Name = dto.Last_Name,
                Company_Name = dto.Company_Name,
                Address_One = dto.Address_One,
                Address_Two = dto.Address_Two,
                City = dto.City,
                Country = dto.Country,
                Postal_Code = dto.Postal_Code
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                throw new Exception("User creation failed.");

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
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
                    user.First_Name,
                    user.Last_Name,
                    user.Email,
                    user.Company_Name,
                    Roles = roles
                });
            }

            return result;
        }

        public async Task UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.ToString()) ?? throw new KeyNotFoundException("User Not Found");
            user.First_Name = dto.FirstName;
            user.Last_Name = dto.LastName;
            user.Email = dto.Email;

            user.Address_One = dto.Address_One;
            user.Address_Two = dto.Address_Two;
            user.City = dto.City;
            user.Country = dto.Country;
            user.Postal_Code = dto.Postal_Code;

            user.Company_Name = dto.Company_Name;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception("Failed to update user");
        }

        public async Task EditUserAsync(string id, EditUserDto dto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id.ToString()) ?? throw new KeyNotFoundException("User Not Found");
            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.First_Name = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.Last_Name = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Address_One))
                user.Address_One = dto.Address_One;

            if (!string.IsNullOrWhiteSpace(dto.Address_Two))
                user.Address_Two = dto.Address_Two;

            if (!string.IsNullOrWhiteSpace(dto.City))
                user.City = dto.City;

            if (!string.IsNullOrWhiteSpace(dto.Country))
                user.Country = dto.Country;

            if (!string.IsNullOrWhiteSpace(dto.Postal_Code))
                user.Postal_Code = dto.Postal_Code;

            if (!string.IsNullOrWhiteSpace(dto.Company_Name))
                user.Company_Name = dto.Company_Name;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception("Failed to update user");
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

    }
}
