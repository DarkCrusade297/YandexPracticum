using EventManagerSystem.DTO.Bookings;

namespace EventManagerSystem.Repositories.Booking
{
    public interface IBookingRepository
    {
        Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId);
    }
}
