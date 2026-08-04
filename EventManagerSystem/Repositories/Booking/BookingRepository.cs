using EventManagerSystem.DataAccess;
using EventManagerSystem.Enums;
using EventManagerSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Repositories.Booking
{
    internal class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<BookingModel?> GetBookingByIdAsync(Guid bookingId)
        {
            return await _db.Bookings.FirstOrDefaultAsync(e => e.Id == bookingId);
        }

        public async Task<BookingModel> CreateBookingAsync(BookingModel booking)
        {
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<IEnumerable<BookingModel>> GetPendingBookingsAsync()
        {
            return _db.Bookings.Where(b => b.Status == Enums.BookingStatus.Pending).ToList();
        }

        public async Task<List<Guid>> GetPendingBookingsIdsAsync()
        {
            return await _db.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .Select(b => b.Id)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
