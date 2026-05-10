using System.Text.Json.Serialization;

namespace EventManagerSystem.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Rejected
    }
}
