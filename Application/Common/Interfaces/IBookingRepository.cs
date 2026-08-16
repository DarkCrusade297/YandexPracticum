using Domain.Models;

namespace Application.Repositories.Booking
{
    public interface IBookingRepository
    {
        Task<BookingModel?> GetBookingByIdAsync(Guid bookingId);
        Task<BookingModel> CreateBookingAsync(BookingModel booking);
        Task<List<BookingModel>> GetPendingBookingsAsync();
        Task<List<Guid>> GetPendingBookingsIdsAsync();
        void UpdateBooking(BookingModel model);
        Task SaveChangesAsync();
    }
}
