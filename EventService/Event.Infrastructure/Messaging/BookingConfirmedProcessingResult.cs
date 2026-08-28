namespace Event.Infrastructure.Messaging;

public enum BookingConfirmedProcessingResult
{
    Processed,
    Duplicate,
    EventNotFound,
    NoAvailableSeats,
    InvalidSeatCount
}
