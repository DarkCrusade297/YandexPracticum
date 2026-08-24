namespace Booking.Domain.Exceptions;

public class BookingLimitExceededException(Guid userId)
    : DomainException($"User '{userId}' has reached the maximum limit of {MaxActiveBookingsPerUser} active bookings")
{
    public const int MaxActiveBookingsPerUser = 10;
}
