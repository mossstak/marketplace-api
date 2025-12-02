using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Dtos
{
    public class CreateOrderDto
    {
        public List<BuyVariantDto> Items { get; set; } = new();
        public string? ShippingAddressLine1 { get; set; }
        public string? ShippingAddressLine2 { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingCountry { get; set; }
        public string? PostalCode { get; set; }
    }
}