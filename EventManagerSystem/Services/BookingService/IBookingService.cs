using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Models;

namespace EventManagerSystem.Services.BookingService
{
    public interface IBookingService
    {
        Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId);
        Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId);
        Task<BookingModel> GetBookingModelByIdAsync(Guid id);
        Task<IEnumerable<BookingModel>> GetPendingBookingsAsync();
        Task UpdateBookingAsync(Guid booking);
        Task RejectBookingAsync(Guid bookingForRejecting);
    }
}
