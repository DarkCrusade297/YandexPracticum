using EventManagerSystem.Enums;

namespace EventManagerSystem.DTO.Bookings
{
    public class GetBookingDto
    {
        public required Guid Id { get; init; }
        public required BookingStatus Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
