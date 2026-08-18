namespace Domain.Exceptions
{
    public class BookingCancelException : DomainException
    {
        public BookingCancelException(string message)
          : base(message)
        {
        }
    }
}
