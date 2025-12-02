using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlaceApi.Dtos;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Services
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(CreateProductDto dto, User seller);
        Task<IEnumerable<object>> GetAllProductsAsync();
        Task UpdateProductAsync(int id, UpdateProductDto dto);
        Task EditProductAsync(int id, EditProductDto dto);
        Task DeleteProductAsync(int id);
    }
}