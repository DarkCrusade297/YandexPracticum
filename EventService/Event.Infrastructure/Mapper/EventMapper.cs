using Event.Domain.Models;
using Event.Infrastructure.Entities;

namespace Event.Infrastructure.Mapper;

public static class EventMapper
{
    public static EventEntity ToEntity(EventModel model) => new()
    {
        Id = model.Id, Title = model.Title, Description = model.Description, StartAt = model.StartAt,
        EndAt = model.EndAt, TotalSeats = model.TotalSeats, AvailableSeats = model.AvailableSeats
    };

    public static EventModel ToDomain(EventEntity entity) =>
        new(entity.Id, entity.Title, entity.Description, entity.StartAt, entity.EndAt, entity.TotalSeats, entity.AvailableSeats);
}
