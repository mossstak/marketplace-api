using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("api/admin/roasters")]
    [Authorize(Roles = "Admin")]
    public class AdminRoastersController : ControllerBase
    {
        private readonly IRoasterProfileService _roasterProfileService;

        public AdminRoastersController(IRoasterProfileService roasterProfileService)
        {
            _roasterProfileService = roasterProfileService;
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateRoasterStatusDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Request body cannot be empty." });
            }

            var status = dto.GetEffectiveStatus();

            try
            {
                var updated = await _roasterProfileService.UpdateApprovalStatusAsync(id, status);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var roasters = await _roasterProfileService.GetAllRoastersForAdminAsync();
            return Ok(roasters);
        }
    }
}
