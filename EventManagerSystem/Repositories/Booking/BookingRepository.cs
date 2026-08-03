using EventManagerSystem.DataAccess;
using EventManagerSystem.DTO.Bookings;
using EventManagerSystem.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace EventManagerSystem.Repositories.Booking
{
    internal class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<GetBookingDto?> GetBookingByIdAsync(Guid bookingId)
        {
            var bk = await _db.Bookings.FirstOrDefaultAsync(e => e.Id == bookingId);
            if (bk is null)
            {
                throw new NotFoundException($"Booking with id {bookingId} not found");
            }
            return new GetBookingDto
            {
                Id = bk.Id,
                Status = bk.Status,
                ProcessedAt = bk.ProcessedAt
            };
        }
    }
}
