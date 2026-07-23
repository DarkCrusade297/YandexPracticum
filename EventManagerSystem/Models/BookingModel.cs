using EventManagerSystem.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EventManagerSystem.Models
{
    public class BookingModel
    {
        public required Guid Id { get; init; } = Guid.NewGuid();
        public required Guid EventId { get; init; }
        public required BookingStatus Status { get; set; } = BookingStatus.Pending;
        public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; } = null!;

        public EventModel Event { get; set; }

        [SetsRequiredMembers]
        public BookingModel(Guid eventId, DateTime? processedAt)
        {
            EventId = eventId;
            ProcessedAt = processedAt;
        }

        [SetsRequiredMembers]
        public BookingModel(Guid id, Guid eventId, BookingStatus status, DateTime? processedAt)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            ProcessedAt = processedAt;
        }

        [SetsRequiredMembers]
        public BookingModel()
        {
        }
    }
}
