using Domain.Enums;

namespace Application.DTO.Bookings
{
    public sealed record BookingDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
}
