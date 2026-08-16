using Domain.Enums;
using Domain.Models;

namespace Application.DTO.Bookings
{
    public class GetBookingDto
    {
        public Guid Id { get; init; }
        public Guid EventId { get; init; }
        public BookingStatus Status { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; init; }

        public static GetBookingDto FromDomain(BookingModel model)
        {
            return new GetBookingDto
            {
                Id = model.Id,
                EventId = model.EventId,
                Status = model.Status,
                CreatedAt = model.CreatedAt,
                ProcessedAt = model.ProcessedAt
            };
        }
    }
}
