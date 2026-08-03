using EventManagerSystem.Models;

namespace EventManagerSystem.Repositories.Booking
{
    public interface IBookingRepository
    {
        Task<BookingModel> GetBookingByIdAsync(Guid bookingId);
        Task<BookingModel> CreateBookingAsync(BookingModel booking);
        Task<IEnumerable<BookingModel>> GetPendingBookingsAsync();
        Task SaveChangesAsync();
    }
}
