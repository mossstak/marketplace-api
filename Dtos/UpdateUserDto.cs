namespace MarketPlaceApi.Dtos
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string AddressOne { get; set; } = "";
        public string AddressTwo { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; }
        public string PostalCode { get; set; } = "";
    }
}
