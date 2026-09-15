using MarketPlaceApi.Models;
using System.Text.Json.Serialization;

namespace MarketPlaceApi.Dtos
{
    public class UpdateRoasterStatusDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApprovalStatus? Status { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApprovalStatus? ApprovalStatus { get; set; }

        public ApprovalStatus GetEffectiveStatus()
        {
            if (Status.HasValue) return Status.Value;
            if (ApprovalStatus.HasValue) return ApprovalStatus.Value;
            return Models.ApprovalStatus.Pending;
        }
    }
}
