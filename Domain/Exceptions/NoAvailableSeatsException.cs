namespace Domain.Exceptions
{
    public class NoAvailableSeatsException : DomainException
    {
        public NoAvailableSeatsException(string message)
          : base(message)
        {
        }
    }
}
