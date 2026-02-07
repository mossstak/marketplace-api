using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketPlaceApi.Services;
using System.Security.Claims;

namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImagesService _productImages;

        public ProductImagesController(IProductImagesService productImages)
        {
            _productImages = productImages;
        }

        private string? ResolveUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("id");

            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }

        public record AttachSellerImageRequest(int SellerImageId, bool IsPrimary);

        [Authorize(Roles = "Seller,Admin")]
        [HttpPost("{productId:int}/images/attach")]
        public async Task<IActionResult> AttachImage(int productId, [FromBody] AttachSellerImageRequest req)
        {
            var userId = ResolveUserId();
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var saved = await _productImages.AttachSellerImageAsync(
                    productId,
                    req.SellerImageId,
                    userId,
                    isAdmin,
                    req.IsPrimary
                );

                return Ok(saved);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Seller,Admin")]
        [HttpPatch("{productId:int}/images/{imageId:int}/primary")]
        public async Task<IActionResult> SetPrimary(int productId, int imageId)
        {
            var userId = ResolveUserId();
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var updated = await _productImages.SetPrimaryImageAsync(
                    productId,
                    imageId,
                    userId,
                    isAdmin
                );
                return Ok(updated);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Seller,Admin")]
        [HttpDelete("{productId:int}/images/{imageId:int}")]
        public async Task<IActionResult> DeleteImage(int productId, int imageId)
        {
            var userId = ResolveUserId();
            var isAdmin = User.IsInRole("Admin");

            try
            {
                await _productImages.DeleteImageAsync(productId, imageId, userId, isAdmin);
                return Ok(new { message = "Image removed." });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}
