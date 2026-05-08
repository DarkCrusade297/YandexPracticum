using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services.BookingService
{
    public interface IBookingService
    {
        Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId);
        Task<BookingModel?> GetBookingByIdAsync(Guid bookingId);
    }
}
