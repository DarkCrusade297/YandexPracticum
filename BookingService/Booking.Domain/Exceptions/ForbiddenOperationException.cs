namespace Booking.Domain.Exceptions;

public class ForbiddenOperationException(string message) : DomainException(message);
