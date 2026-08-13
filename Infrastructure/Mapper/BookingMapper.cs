using EventManagerSystem.Models;
using Infrastructure.Entities;

namespace Infrastructure.Mapper
{
    public class BookingMapper
    {
        public static BookingEntity ToEntity(BookingModel domain)
        {
            return new BookingEntity
            {
                Id = domain.Id,
                EventId = domain.EventId,
                CreatedAt = domain.CreatedAt,
                Status = domain.Status,
                ProcessedAt = domain.ProcessedAt,
            };
        }

        public static BookingModel ToDomain(BookingEntity entity)
        {
            var model = new BookingModel(entity.Id, entity.EventId, entity.Status, entity.CreatedAt, entity.ProcessedAt);
            return model;
        }
    }
}
