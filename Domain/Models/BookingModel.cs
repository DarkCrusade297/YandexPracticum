using Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Models
{
    public class BookingModel
    {
        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
        public Guid UserId { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        public EventModel? Event { get; private set; }
        public UserModel? User { get; private set; }

        public BookingModel(Guid eventId, Guid userId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            Status = BookingStatus.Pending;   
            CreatedAt = DateTime.UtcNow;
            ProcessedAt = null;               
        }
        public BookingModel(Guid id, Guid eventId, Guid userId, BookingStatus status, DateTime createdAt, DateTime? processedAt)
        {
            Id = id;
            EventId = eventId;
            UserId = userId;
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
