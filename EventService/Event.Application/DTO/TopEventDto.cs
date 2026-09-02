using Event.Domain.Models;

namespace Event.Application.DTO;

public sealed class TopEventDto : EventDto
{
    public double SoldPercentage { get; init; }

    public static TopEventDto FromEvent(EventModel model) => new()
    {
        Id = model.Id,
        Title = model.Title,
        Description = model.Description,
        StartAt = model.StartAt,
        EndAt = model.EndAt,
        TotalSeats = model.TotalSeats,
        AvailableSeats = model.AvailableSeats,
        SoldPercentage = model.TotalSeats > 0
            ? (double)(model.TotalSeats - model.AvailableSeats) / model.TotalSeats
            : 0
    };
}
