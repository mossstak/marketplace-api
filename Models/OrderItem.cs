using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Models
{
    public class OrderItem
    {
        public int Id {get; set;}
        public int OrderId {get; set;}
        public Order? Order {get; set;}
        public int ProductVariantId {get; set;}
        public ProductVariant? Variant {get; set;}
        public int Quantity {get; set;}
        public decimal UnitPrice {get; set;}
        public decimal Subtotal => UnitPrice * Quantity;
    }
}