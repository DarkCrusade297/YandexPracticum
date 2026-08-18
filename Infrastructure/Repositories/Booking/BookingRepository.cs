using Infrastructure.DataAccess;
using Domain.Enums;
using Domain.Models;
using Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;
using Application.Repositories.Booking;

namespace Infrastructure.Repositories.Booking
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
            var bk = await _db.Bookings.FirstOrDefaultAsync(e => e.Id == bookingId);
            if (bk is null)
                return null;
            return BookingMapper.ToDomain(bk);
        }

        public async Task<BookingModel> CreateBookingAsync(BookingModel booking)
        {
            var bk = BookingMapper.ToEntity(booking);
            _db.Bookings.Add(bk);
            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<List<BookingModel>> GetPendingBookingsAsync()
        {
            var bks = await _db.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync();
            return bks.Select(e => BookingMapper.ToDomain(e)).ToList();
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

        public void UpdateBooking(BookingModel model)
        {
            var entity = _db.Bookings.Local.FirstOrDefault(b => b.Id == model.Id)
                         ?? _db.Bookings.Find(model.Id)
                         ?? throw new InvalidOperationException($"Booking {model.Id} not found");

            entity.Status = model.Status;
            entity.ProcessedAt = model.ProcessedAt;
        }

        public async Task<int> CountActiveBookingsByUserIdAsync(Guid userId)
        {
            return await _db.Bookings.CountAsync(b => b.UserId == userId &&(b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
        }
    }
}
