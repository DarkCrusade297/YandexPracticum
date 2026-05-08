using EventManagerSystem.Enums;

namespace EventManagerSystem.DTO.Bookings
{
    public class CreatedBookingDto
    {
        public required Guid Id { get; init; }
        public required Guid EventId { get; init; }
        public required BookingStatus Status { get; init; }
    }
}
