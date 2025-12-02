using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Controllers;
using MarketPlaceApi.Models;
using MarketPlaceApi.Dtos;
namespace MarketPlaceApi.Services
{
    public interface IUserService
    {
        Task<User> Register(RegisterDto dto);
        Task<User> Login(LoginDto dto);
        Task<IEnumerable<object>> GetAllUser();
        Task UpdateUserAsync(string id, UpdateUserDto dto);
        Task EditUserAsync(string id, EditUserDto dto);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task ResetPasswordAsync(string userId, string newPassword);
    }
}