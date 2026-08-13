using EventManagerSystem.DataAccess;
using EventManagerSystem.Enums;
using EventManagerSystem.Models;
using Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;

namespace EventManagerSystem.Repositories.Booking
{
    public class BookingRepository : IBookingRepository
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
            var bk = BookingMapper.ToEntity(booking);
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            return BookingMapper.ToDomain(bk);
        }

        public async Task<IEnumerable<BookingModel>> GetPendingBookingsAsync()
        {
            return await _db.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync();
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
