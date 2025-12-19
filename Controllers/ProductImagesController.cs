using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketPlaceApi.Services;

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

        [Authorize(Roles = "Seller,Admin")]
        [HttpPost("{productId:int}/images/sign")]
        public async Task<IActionResult> SignUpload(int productId)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var signed = await _productImages.CreateUploadSignatureAsync(productId, userId, isAdmin);
                return Ok(signed);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        public record SaveProductImageRequest(string ImageUrl, string PublicId, bool IsPrimary);

        [Authorize(Roles = "Seller,Admin")]
        [HttpPost("{productId:int}/images")]
        public async Task<IActionResult> SaveImage(int productId, [FromBody] SaveProductImageRequest req)
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole("Admin");

            try
            {
                var saved = await _productImages.SaveProductImageAsync(
                    productId,
                    userId,
                    isAdmin,
                    req.ImageUrl,
                    req.PublicId,
                    req.IsPrimary
                );

                return Ok(saved);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }
    }
}
