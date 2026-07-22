using System.Diagnostics.CodeAnalysis;

namespace EventManagerSystem.Models
{
    public class EventModel
    {
        public required Guid Id { get; init; } = Guid.NewGuid();
        public required string? Title { get; set; }
        public string? Description { get; set; }
        public required DateTime? StartAt { get; set; }
        public required DateTime? EndAt { get; set; }
        public required int? TotalSeats { get; set; }
        public required int? AvailableSeats { get; set; }

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
    }
}
