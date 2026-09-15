using MarketPlaceApi.Dtos;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("api/[controller]")]
    [Route("api/RoasterProfiles")]
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
            if (profile == null)
            {
                return NotFound(new { message = "Roaster profile not found." });
            }
            return Ok(profile);
        }

        [HttpPost("become-roaster")]
        [Authorize]
        public async Task<IActionResult> BecomeRoaster([FromBody] BecomeRoasterDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userId)) return Unauthorized("No User Id claim found.");

            try
            {
                var result = await _service.BecomeRoasterAsync(userId, dto);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
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
        // GET /RoasterProfile/all
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
