using Application.DTO.Bookings;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.BookingService
{
    public interface IBookingService
    {
        Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId, Guid userId);
        Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId, Guid currentUserId, UserRoles currentUserRole);
        Task<BookingModel> GetBookingModelByIdAsync(Guid id);
        Task<IEnumerable<BookingModel>> GetPendingBookingsAsync();
        Task UpdateBookingAsync(Guid bookingId);
        Task RejectBookingAsync(Guid bookingForRejectingId);
        Task CancelBookingAsync(Guid bookingId, Guid currentUserId, UserRoles currentUserRole);
    }
}
