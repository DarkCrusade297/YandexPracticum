using Booking.Domain.Models;
using Booking.Infrastructure.Entities;

namespace Booking.Infrastructure.Mapper;

public static class BookingMapper
{
    public static BookingEntity ToEntity(BookingModel model) => new()
    {
        Id = model.Id, EventId = model.EventId, UserId = model.UserId,
        Status = model.Status, CreatedAt = model.CreatedAt, ProcessedAt = model.ProcessedAt
    };

    public static BookingModel ToDomain(BookingEntity entity) =>
        new(entity.Id, entity.EventId, entity.UserId, entity.Status, entity.CreatedAt, entity.ProcessedAt);
}
