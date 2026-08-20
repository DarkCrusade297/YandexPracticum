using System.Text.Json.Serialization;

namespace Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Cancelled
    }
}
