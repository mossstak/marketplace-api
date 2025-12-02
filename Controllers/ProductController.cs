using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Data;
using MarketPlaceApi.Services;


namespace MarketPlaceApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly UserManager<User> _userManager;

        public ProductController(
            IProductService productService,
            UserManager<User> userManager)
        {
            _productService = productService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Seller, Admin")]
        [HttpPost("addproduct")]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            // Get current user as seller
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            try
            {
                var product = await _productService.CreateProductAsync(dto, user);
                return Ok(new { product.Id, Message = "Product Created." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("all")]
        public async Task<IActionResult> GetProduct()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [Authorize(Roles = "Seller, Admin")]
        [HttpPatch("editproduct/{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] EditProductDto dto)
        {
            try
            {
                await _productService.EditProductAsync(id, dto);
                return Ok("Product has been changed");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Seller, Admin")]
        [HttpPut("updateproduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                await _productService.UpdateProductAsync(id, dto);
                return Ok("Product Updated");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Seller, Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok("Product Deleted");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}