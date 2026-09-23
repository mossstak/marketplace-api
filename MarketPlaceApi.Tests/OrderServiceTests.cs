using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketPlaceApi.Data;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class OrderServiceTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsException_WhenBuyerTriesToBuyTheirOwnProduct()
    {
        using var context = CreateDbContext();

        var seller = new User { Id = "seller-123", UserName = "seller@coffee.com", Email = "seller@coffee.com" };
        var buyer = seller; // Same user

        var product = new Product
        {
            Id = 1,
            ProductName = "Test Artisan Coffee",
            SellerId = seller.Id
        };

        var variant = new ProductVariant
        {
            Id = 10,
            ProductId = product.Id,
            Product = product,
            Size = "250g",
            Price = 12.50m,
            Quantity = 10
        };

        await context.Users.AddAsync(seller);
        await context.Products.AddAsync(product);
        await context.ProductVariants.AddAsync(variant);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var dto = new CreateOrderDto
        {
            Items = new List<BuyVariantDto>
            {
                new BuyVariantDto { VariantId = variant.Id, Quantity = 1 }
            }
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrderAsync(buyer, dto));
        Assert.Equal("You cannot purchase your own products.", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_Succeeds_WhenBuyerBuysDifferentSellerProduct()
    {
        using var context = CreateDbContext();

        var seller = new User { Id = "seller-1", UserName = "seller@coffee.com", Email = "seller@coffee.com" };
        var buyer = new User { Id = "buyer-1", UserName = "buyer@customer.com", Email = "buyer@customer.com" };

        var product = new Product
        {
            Id = 2,
            ProductName = "Colombian Special",
            SellerId = seller.Id
        };

        var variant = new ProductVariant
        {
            Id = 20,
            ProductId = product.Id,
            Product = product,
            Size = "500g",
            Price = 18.00m,
            Quantity = 10
        };

        await context.Users.AddRangeAsync(seller, buyer);
        await context.Products.AddAsync(product);
        await context.ProductVariants.AddAsync(variant);
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var dto = new CreateOrderDto
        {
            Items = new List<BuyVariantDto>
            {
                new BuyVariantDto { VariantId = variant.Id, Quantity = 2 }
            }
        };

        var order = await service.CreateOrderAsync(buyer, dto);

        Assert.NotNull(order);
        Assert.Equal(buyer.Id, order.BuyerId);
        Assert.Equal(36.00m, order.TotalAmount);
        Assert.Equal(8, variant.Quantity); // 10 - 2
    }
}
