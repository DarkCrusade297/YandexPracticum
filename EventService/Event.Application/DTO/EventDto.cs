using Event.Domain.Models;

namespace Event.Application.DTO;

public class EventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public int TotalSeats { get; init; }
    public int AvailableSeats { get; init; }

    public static EventDto FromDomain(EventModel model) => new()
    {
        Id = model.Id, Title = model.Title, Description = model.Description, StartAt = model.StartAt,
        EndAt = model.EndAt, TotalSeats = model.TotalSeats, AvailableSeats = model.AvailableSeats
    };
}
