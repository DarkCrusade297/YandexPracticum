namespace Domain.Exceptions
{
    public class BookingLimitExceededException : DomainException
    {
        public const int MaxActiveBookingsPerUser = 10;

        public BookingLimitExceededException(Guid userId)
            : base($"User '{userId}' has reached the maximum limit of {MaxActiveBookingsPerUser} active bookings")
        {
        }
    }
}
