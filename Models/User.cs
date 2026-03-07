using Microsoft.AspNetCore.Identity;

namespace MarketPlaceApi.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressOne {get; set;}
        public string? AddressTwo {get; set;}
        public string? City {get; set;}
        public string? Country {get; set;}
        public string? PostalCode {get; set;}

        public RoasterProfile? RoasterProfile { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}