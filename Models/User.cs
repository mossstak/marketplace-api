using Microsoft.AspNetCore.Identity;

namespace MarketPlaceApi.Models
{
    public class User : IdentityUser
    {
        public string? First_Name { get; set; }
        public string? Last_Name { get; set; }

        public string Company_Name {get; set;}
        public string Address_One {get; set;}
        public string Address_Two {get; set;}
        public string City {get; set;}
        public string Country {get; set;}
        public string Postal_Code {get; set;}

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}