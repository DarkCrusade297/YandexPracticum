using System.Diagnostics.CodeAnalysis;

namespace EventManagerSystem.Models
{
    public class EventModel
    {
        public required Guid Id { get; init; } = Guid.NewGuid();
        public required string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public required DateTime? StartAt { get; set; } = null!;
        public required DateTime? EndAt { get; set; } = null!;
        public required int? TotalSeats { get; set; } = null!;
        public required int? AvailableSeats { get; set; } = null!;

        public ICollection<BookingModel> bookingModels { get; set; } = new List<BookingModel>();

        [SetsRequiredMembers]
        public EventModel(string? title, string? description, DateTime? startAt, DateTime? endAt, int? totalSeats)
        {
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
        }

        [SetsRequiredMembers]
        public EventModel()
        {
        }
    }
}
