namespace Messaging.Contracts.Bookings;

public sealed record BookingConfirmed(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int SeatCount,
    DateTimeOffset ConfirmedAt);

