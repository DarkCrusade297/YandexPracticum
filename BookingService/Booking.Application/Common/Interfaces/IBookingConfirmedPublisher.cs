using Messaging.Contracts.Bookings;

namespace Booking.Application.Common.Interfaces;

public interface IBookingConfirmedPublisher
{
    Task PublishAsync(BookingConfirmed message, CancellationToken cancellationToken = default);
}

