namespace Domain.Exceptions
{
    public class BookingCancelException : Exception
    {
        public BookingCancelException() { }
        public BookingCancelException(string message)
          : base(message)
        {
        }
        public BookingCancelException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}
