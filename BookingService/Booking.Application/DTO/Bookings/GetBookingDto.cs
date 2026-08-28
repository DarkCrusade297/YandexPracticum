using Booking.Domain.Enums;
using Booking.Domain.Models;

namespace Booking.Application.DTO.Bookings;

public class GetBookingDto
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public BookingStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }

    public static GetBookingDto FromDomain(BookingModel model) => new()
    {
        Id = model.Id, EventId = model.EventId, UserId = model.UserId, Status = model.Status,
        CreatedAt = model.CreatedAt, ProcessedAt = model.ProcessedAt
    };
}
