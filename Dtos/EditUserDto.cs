namespace MarketPlaceApi.Dtos
{
    public class EditUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        // Address fields (optional for PATCH)
        public string? Address_One { get; set; }
        public string? Address_Two { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Postal_Code { get; set; }
        // Only sellers have this
        public string? Company_Name { get; set; }
    }
}
