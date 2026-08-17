namespace Domain.Exceptions
{
    public class EventAlreadyPassedException : DomainException
    {
        public EventAlreadyPassedException(Guid eventId)
            : base($"Cannot create a booking for event '{eventId}' because it has already taken place")
        {
        }
    }
}
