using System.Text.Json.Serialization;

namespace User.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRoles
{
    User,
    Admin
}
