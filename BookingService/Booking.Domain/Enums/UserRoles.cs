using System.Text.Json.Serialization;

namespace Booking.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRoles
{
    User,
    Admin
}
