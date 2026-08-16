using Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Models
{
    public class BookingModel
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        public EventModel? Event { get; private set; }

        public BookingModel(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;   
            CreatedAt = DateTime.UtcNow;
            ProcessedAt = null;               
        }
        public BookingModel(Guid id, Guid eventId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
        }

        public void UpdateStatus(BookingStatus status)
        {
            Status = status;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}
