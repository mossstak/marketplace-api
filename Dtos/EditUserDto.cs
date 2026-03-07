namespace MarketPlaceApi.Dtos
{
    public class EditUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        // Address fields (optional for PATCH)
        public string? AddressOne { get; set; }
        public string? AddressTwo { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        // Only sellers have this
        public string? Company_Name { get; set; }
    }
}
