using Booking.Domain.Enums;

namespace Booking.Application.DTO.Bookings;

public class CreatedBookingDto
{
    public required Guid Id { get; init; }
    public required Guid EventId { get; init; }
    public required Guid UserId { get; init; }
    public required BookingStatus Status { get; set; }
}
