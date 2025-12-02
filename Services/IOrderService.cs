using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(User buyer, CreateOrderDto dto);
        Task UpdateOrderAsync(User buyer, int orderId, CreateOrderDto dto);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task DeleteOrderAsync(int orderId, string requesterId, bool isAdmin);
        Task<IEnumerable<object>> GetOrdersForUserAsync(string buyerId);
    }
}
