using Application.Entites;

namespace EventManagerSystem.Repositories.Booking
{
    public interface IBookingRepository
    {
        Task<BookingDto?> GetBookingByIdAsync(Guid bookingId);
        Task<BookingDto> CreateBookingAsync(BookingDto booking);
        Task<IEnumerable<BookingDto>> GetPendingBookingsAsync();
        Task<List<Guid>> GetPendingBookingsIdsAsync();
        Task SaveChangesAsync();
    }
}
