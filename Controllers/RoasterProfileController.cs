using MarketPlaceApi.Dtos;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoasterProfileController : ControllerBase
    {
        private readonly IRoasterProfileService _service;

        public RoasterProfileController(IRoasterProfileService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("No User Id claim found.");

            var profile = await _service.GetMyProfileAsync(userId);
            return Ok(profile);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpsertMe([FromBody] UpsertRoasterProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("No User Id claim found.");

            var profile = await _service.UpsertMyProfileAsync(userId, dto);
            return Ok(profile);
        }

        // Public storefront fetch (no auth)
        // GET /RoasterProfile/public
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoaster()
        {
            var roasters = await _service.GetAllRoaster();
            return Ok(roasters);
        }
        
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetPublic(string userId)
        {
            try
            {
                var profile = await _service.GetPublicByUserIdAsync(userId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // Admin verification toggle
        [Authorize(Roles = "Admin")]
        [HttpPost("verify/{userId}")]
        public async Task<IActionResult> Verify(string userId, [FromBody] VerifyRoasterProfileDto dto)
        {
            var updated = await _service.SetVerificationAsync(userId, dto.IsVerified);
            return Ok(updated);
        }
    }
}
