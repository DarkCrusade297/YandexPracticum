using EventManagerSystem.Models;
using Infrastructure.Entities;

namespace Infrastructure.Mapper
{
    public static class EventMapper
    {
        public static EventEntity ToEntity(EventModel domain) => new()
        {
            Id = domain.Id,
            Title = domain.Title,
            Description = domain.Description,
            StartAt = domain.StartAt,
            EndAt = domain.EndAt,
            TotalSeats = domain.TotalSeats,
            AvailableSeats = domain.AvailableSeats
        };

        public static EventModel ToDomain(EventEntity entity)
        {
            var model = new EventModel(entity.Id, entity.Title, entity.Description, entity.StartAt, entity.EndAt, entity.TotalSeats, entity.AvailableSeats);
            return model;
        }
    }
}
