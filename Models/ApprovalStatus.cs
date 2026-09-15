using System.Text.Json.Serialization;

namespace MarketPlaceApi.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
