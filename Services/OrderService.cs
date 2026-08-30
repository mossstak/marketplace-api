using System;
using System.Collections.Generic;
using System.Linq;
using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(User buyer, CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        var variantIds = dto.Items.Select(i => i.VariantId).ToList();
        var variants = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync();

        if (variants.Count != variantIds.Distinct().Count())
            throw new KeyNotFoundException("One or more product variants were not found.");

        var order = new Order
        {
            BuyerId = buyer.Id,
            Status = OrderStatus.Pending
        };

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            var variant = variants.First(v => v.Id == item.VariantId);

            if (variant.Quantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for variant {variant.Id}.");

            var unitPrice = variant.Price;

            order.Items.Add(new OrderItem
            {
                ProductVariantId = variant.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice
            });

            variant.Quantity -= item.Quantity;
            order.TotalAmount += unitPrice * item.Quantity;
        }

        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateOrderAsync(User buyer, int orderId, CreateOrderDto dto)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");

        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!string.Equals(order.BuyerId, buyer.Id, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("You can only update your own orders.");

        // Restore stock for existing items
        var existingVariantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var existingVariants = await _context.ProductVariants
            .Where(v => existingVariantIds.Contains(v.Id))
            .ToListAsync();

        foreach (var item in order.Items)
        {
            var variant = existingVariants.First(v => v.Id == item.ProductVariantId);
            variant.Quantity += item.Quantity;
        }

        // Validate new variants
        var newVariantIds = dto.Items.Select(i => i.VariantId).ToList();
        var newVariants = await _context.ProductVariants
            .Where(v => newVariantIds.Contains(v.Id))
            .ToListAsync();

        if (newVariants.Count != newVariantIds.Distinct().Count())
            throw new KeyNotFoundException("One or more product variants were not found.");

        order.Items.Clear();
        order.TotalAmount = 0;

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new InvalidOperationException("Quantity must be greater than zero.");

            var variant = newVariants.First(v => v.Id == item.VariantId);

            if (variant.Quantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for variant {variant.Id}.");

            var unitPrice = variant.Price;
            order.Items.Add(new OrderItem
            {
                ProductVariantId = variant.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice
            });

            variant.Quantity -= item.Quantity;
            order.TotalAmount += unitPrice * item.Quantity;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        order.Status = status;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSellerOrderStatusAsync(int orderId, OrderStatus newStatus, string sellerId, bool isAdmin)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Variant)
            .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        // Validate that the order contains items belonging to this seller
        if (!isAdmin)
        {
            bool ownsAnyItem = order.Items.Any(i => i.Variant?.Product?.SellerId == sellerId);
            if (!ownsAnyItem)
            {
                throw new UnauthorizedAccessException("You can only update orders for your own products.");
            }
        }

        order.Status = newStatus;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrderAsync(int orderId, string requesterId, bool isAdmin)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (!isAdmin && !string.Equals(order.BuyerId, requesterId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("You can only delete your own orders.");

        var variantIds = order.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await _context.ProductVariants
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync();

        foreach (var item in order.Items)
        {
            var variant = variants.First(v => v.Id == item.ProductVariantId);
            variant.Quantity += item.Quantity;
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<object>> GetOrdersForSellerAsync(string sellerId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
                .ThenInclude(v => v.Product)
            .Where(o => o.Items.Any(i => i.Variant != null && i.Variant.Product != null && i.Variant.Product.SellerId == sellerId))
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,
                Items = o.Items
                    .Where(i => i.Variant != null && i.Variant.Product != null && i.Variant.Product.SellerId == sellerId)
                    .Select(i => new
                    {
                        i.Id,
                        i.ProductVariantId,
                        ProductName = i.Variant != null && i.Variant.Product != null ? i.Variant.Product.ProductName : "Artisan Roast",
                        Size = i.Variant != null ? i.Variant.Size : null,
                        i.Quantity,
                        i.UnitPrice,
                        i.Subtotal
                    })
            })
            .ToListAsync();

        return orders.Cast<object>();
    }

    public async Task<IEnumerable<object>> GetOrdersForUserAsync(string buyerId)
    {
        var orders = await _context.Orders
            .Where(o => o.BuyerId == buyerId)
            .Include(o => o.Items)
            .ThenInclude(i => i.Variant)
            .ThenInclude(v => v.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,
                Items = o.Items.Select(i => new
                {
                    i.Id,
                    i.ProductVariantId,
                    ProductName = i.Variant != null && i.Variant.Product != null ? i.Variant.Product.ProductName : "Unknown Coffee",
                    Size = i.Variant != null ? i.Variant.Size : null,
                    i.Quantity,
                    i.UnitPrice,
                    i.Subtotal
                })
            })
            .ToListAsync();

        return orders.Cast<object>();
    }
}
