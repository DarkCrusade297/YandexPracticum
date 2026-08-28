using Booking.Domain.Models;

namespace Booking.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task<BookingModel?> GetBookingByIdAsync(Guid bookingId);
    Task<BookingModel> CreateBookingAsync(BookingModel booking);
    Task<int> CountActiveBookingsByUserIdAsync(Guid userId);
    Task<List<BookingModel>> GetPendingBookingsAsync();
    Task<List<Guid>> GetPendingBookingsIdsAsync();
    void UpdateBooking(BookingModel model);
    Task SaveChangesAsync();
}
