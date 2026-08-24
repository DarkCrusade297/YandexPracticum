namespace Booking.Domain.Exceptions;

public class EventAlreadyPassedException(Guid eventId)
    : DomainException($"Cannot create a booking for event '{eventId}' because it has already taken place");
