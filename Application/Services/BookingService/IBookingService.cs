using Application.DTO.Bookings;
using Domain.Models;

namespace Application.Services.BookingService
{
    public interface IBookingService
    {
        Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId);
        Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId);
        Task<BookingModel> GetBookingModelByIdAsync(Guid id);
        Task<IEnumerable<BookingModel>> GetPendingBookingsAsync();
        Task UpdateBookingAsync(Guid bookingId);
        Task RejectBookingAsync(Guid bookingForRejectingId);
    }
}
