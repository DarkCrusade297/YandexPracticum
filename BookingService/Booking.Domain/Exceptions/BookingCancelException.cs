namespace Booking.Domain.Exceptions;

public class BookingCancelException(string message) : DomainException(message);
