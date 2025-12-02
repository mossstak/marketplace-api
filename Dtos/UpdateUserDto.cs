namespace MarketPlaceApi.Dtos
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address_One { get; set; } = "";
        public string Address_Two { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; }
        public string Postal_Code { get; set; } = "";
        public string Company_Name { get; set; }
    }
}
