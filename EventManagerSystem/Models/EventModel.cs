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

        [SetsRequiredMembers]
        public EventModel(string? title, string? description, DateTime? startAt, DateTime? endAt)
        {
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
