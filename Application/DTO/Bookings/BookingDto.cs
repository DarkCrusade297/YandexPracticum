using Domain.Enums;

namespace Application.DTO.Bookings
{
    public sealed record BookingDto(
    Guid Id,
    Guid EventId,
    BookingStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
}
