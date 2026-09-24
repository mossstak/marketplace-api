using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
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
        private readonly ICloudinarySigner _cloudinarySigner;

        public UserController(
            IUserService userService,
            UserManager<User> userManager,
            TokenService tokenService,
            ICloudinarySigner cloudinarySigner
        )
        {
            _userService = userService;
            _userManager = userManager;
            _tokenService = tokenService;
            _cloudinarySigner = cloudinarySigner;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role == "Admin")
            {
                // Admin accounts must not be self-service registered.
                return BadRequest("Role must be Seller or Buyer.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Role) && dto.Role != "Buyer" && dto.Role != "Seller")
            {
                return BadRequest("Role must be Seller or Buyer.");
            }

            // All new accounts are assigned the default "Buyer" role
            dto.Role = "Buyer";

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

                return Ok(new { roles, token, userId = user.Id });
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

        // For DashBoard
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No User Id claim found.");

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var hasRoasterProfile = user.RoasterProfile != null;

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.AddressOne,
                user.AddressTwo,
                user.City,
                user.Country,
                user.PostalCode,
                user.ProfileImageUrl,
                CompanyName = user.RoasterProfile?.CompanyName,
                Roles = roles,
                HasRoasterProfile = hasRoasterProfile
            });
        }

        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> EditMe([FromBody] EditUserDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No User Id claim found.");

            try
            {
                await _userService.EditUserAsync(userId, dto);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public record UpdateProfileImageRequest(string ImageUrl);

        [Authorize]
        [HttpPost("profile-image/sign")]
        public IActionResult SignProfileImageUpload()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No User Id claim found.");

            var folder = $"marketplace/users/{userId}";
            var signed = _cloudinarySigner.CreateUploadSignature(folder);
            return Ok(signed);
        }

        [Authorize]
        [HttpPost("profile-image")]
        public async Task<IActionResult> UpdateProfileImage([FromBody] UpdateProfileImageRequest dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No User Id claim found.");

            if (string.IsNullOrWhiteSpace(dto.ImageUrl))
                return BadRequest("Image URL is required.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.ProfileImageUrl = dto.ImageUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed to update profile image: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { message = "Profile image updated successfully.", profileImageUrl = user.ProfileImageUrl });
        }

        [Authorize]
        [HttpDelete("profile-image")]
        public async Task<IActionResult> DeleteProfileImage()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("No User Id claim found.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.ProfileImageUrl = null;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed to remove profile image: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { message = "Profile image removed successfully." });
        }

        [Authorize]
        [HttpPatch("edituser/{id}")]
        public async Task<IActionResult> EditUser(string id, [FromBody] EditUserDto dto)
        {
            if (!IsSelfOrAdmin(id))
                return Forbid();

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

        [Authorize]
        [HttpPut("updateuser/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            if (!IsSelfOrAdmin(id))
                return Forbid();

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
            var userId = GetCurrentUserId();
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _userService.ForgotPasswordAsync(dto.Email);
                return Ok(new { message = "If your email is registered, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _userService.ResetPasswordAsync(dto);
                return Ok(new { message = "Password has been successfully reset. You can now log in." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string? GetCurrentUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        private bool IsSelfOrAdmin(string targetUserId) =>
            string.Equals(GetCurrentUserId(), targetUserId, StringComparison.Ordinal) || User.IsInRole("Admin");

    }
}