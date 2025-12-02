using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;

        public UserController(
            IUserService userService,
            UserManager<User> userManager,
            TokenService tokenService
        )
        {
            _userService = userService;
            _userManager = userManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            // 1) Validate based on role
            if (dto.Role == "Seller" && string.IsNullOrWhiteSpace(dto.Company_Name))
            {
                return BadRequest("Company name is required for sellers.");
            }

            if ((dto.Role == "Seller" || dto.Role == "Buyer"))
            {
                if (string.IsNullOrWhiteSpace(dto.Address_One)
                    || string.IsNullOrWhiteSpace(dto.City)
                    || string.IsNullOrWhiteSpace(dto.Country)
                    || string.IsNullOrWhiteSpace(dto.Postal_Code))
                {
                    return BadRequest("Address fields are required for buyers and sellers.");
                }
            }
            // For Admin: no address / company required
            try
            {
                var user = await _userService.Register(dto);
                return Ok("User Registered");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var user = await _userService.Login(dto);
                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.CreateToken(user, roles);

                return Ok(new { roles, token });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAllUser();
            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("edituser/{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] EditUserDto dto)
        {
            try
            {
                await _userService.EditUserAsync(id, dto);
                return Ok("User details has been changed");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("updateuser/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                await _userService.UpdateUserAsync(id, dto);
                return Ok("User Details has been updated");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User Not Found!");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User Deleted");
        }

        [Authorize] // user must be logged in
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
                return Unauthorized();

            try
            {
                await _userService.ChangePasswordAsync(userId, dto);
                return Ok("Password changed successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _userService.ResetPasswordAsync(id, dto.NewPassword);
                return Ok("Password has been reset");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}