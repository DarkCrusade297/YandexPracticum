namespace Event.Infrastructure.Entities;

public sealed class ProcessedBookingEntity
{
    public Guid BookingId { get; set; }
    public Guid EventId { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
