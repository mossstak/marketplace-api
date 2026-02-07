using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("seller/images")]
    public class SellerImagesController : ControllerBase
    {
        private readonly ISellerImagesService _sellerImages;

        public SellerImagesController(ISellerImagesService sellerImages)
        {
            _sellerImages = sellerImages;
        }

        private string? ResolveUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("id");

            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }

        public record SaveSellerImageRequest(string ImageUrl, string PublicId);

        [Authorize(Roles = "Seller,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetMyImages()
        {
            var userId = ResolveUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User not authenticated." });

            var images = await _sellerImages.GetSellerImagesAsync(userId);
            return Ok(images);
        }

        [Authorize(Roles = "Seller,Admin")]
        [HttpPost("sign")]
        public async Task<IActionResult> SignUpload()
        {
            var userId = ResolveUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User not authenticated." });

            var signed = await _sellerImages.CreateUploadSignatureAsync(userId);
            return Ok(signed);
        }

        [Authorize(Roles = "Seller,Admin")]
        [HttpPost]
        public async Task<IActionResult> SaveImage([FromBody] SaveSellerImageRequest req)
        {
            var userId = ResolveUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User not authenticated." });

            var saved = await _sellerImages.SaveSellerImageAsync(userId, req.ImageUrl, req.PublicId);
            return Ok(saved);
        }

        [Authorize(Roles = "Seller,Admin")]
        [HttpDelete("{imageId:int}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var userId = ResolveUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "User not authenticated." });

            try
            {
                await _sellerImages.DeleteSellerImageAsync(userId, imageId);
                return Ok(new { message = "Image removed." });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }
    }
}
