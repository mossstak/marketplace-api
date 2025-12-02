using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;

        public OrderController(IOrderService orderService, UserManager<User> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Buyer")]
        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var order = await _orderService.CreateOrderAsync(user, dto);
                return Ok(new { order.Id, order.TotalAmount, order.Status });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex) // Insufficient Stock
            {
                return NotFound(ex.Message); // Variant Not Found
            }
        }

        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var orders = await _orderService.GetOrdersForUserAsync(user.Id);
            return Ok(orders);
        }

        [Authorize(Roles = "Buyer")]
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] CreateOrderDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                await _orderService.UpdateOrderAsync(user, id, dto);
                return Ok("Order updated");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, dto.Status);
                return Ok("Order status updated");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Buyer, Admin")]
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            try
            {
                await _orderService.DeleteOrderAsync(id, user.Id, isAdmin);
                return Ok("Order deleted");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

    }
}
