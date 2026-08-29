using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MarketPlaceApi.Dtos
{
    public class CreateOrderDto
    {
        [Required]
        public List<BuyVariantDto> Items { get; set; } = new();
        [Required]
        public string? ShippingAddressLine1 { get; set; }
        public string? ShippingAddressLine2 { get; set; }
        [Required]
        public string? ShippingCity { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country must be a 2-letter ISO country code (e.g., GB, US).")]
        public string ShippingCountry { get; set; } = "GB";
        [Required]
        public string? PostalCode { get; set; }
    }
}