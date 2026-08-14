using Domain.Enums;

namespace Infrastructure.Entities
{
    public class BookingEntity
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public EventEntity Event { get; set; } = null!;
    }
}
