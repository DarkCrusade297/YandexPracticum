using EventManagerSystem.Enums;

namespace Application.Entites
{
    public sealed record BookingDto(
    Guid Id,
    Guid EventId,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
}
