using Booking.Application.DTO.Events;

namespace Booking.Application.Common.Interfaces;

public interface IEventGateway
{
    Task<EventDto> GetEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task ReserveSeatAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task ReleaseSeatAsync(Guid eventId, CancellationToken cancellationToken = default);
}
