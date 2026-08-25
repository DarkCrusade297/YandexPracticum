using Booking.Application.DTO.Bookings;
using Booking.Domain.Enums;
using Booking.Domain.Models;

namespace Booking.Application.Services;

public interface IBookingService
{
    Task<CreatedBookingDto?> CreateBookingAsync(Guid eventId, Guid userId);
    Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId, Guid currentUserId, UserRoles currentUserRole);
    Task<BookingModel> GetBookingModelByIdAsync(Guid id);
    Task<IEnumerable<BookingModel>> GetPendingBookingsAsync();
    Task UpdateBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task CancelBookingAsync(Guid bookingId, Guid currentUserId, UserRoles currentUserRole);
}
