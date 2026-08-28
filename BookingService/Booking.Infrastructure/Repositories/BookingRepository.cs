using Booking.Application.Common.Interfaces;
using Booking.Domain.Enums;
using Booking.Domain.Models;
using Booking.Infrastructure.DataAccess;
using Booking.Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Repositories;

public class BookingRepository(BookingDbContext db) : IBookingRepository
{
    public async Task<BookingModel?> GetBookingByIdAsync(Guid id)
    {
        var entity = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        return entity is null ? null : BookingMapper.ToDomain(entity);
    }

    public async Task<BookingModel> CreateBookingAsync(BookingModel booking)
    {
        db.Bookings.Add(BookingMapper.ToEntity(booking));
        await db.SaveChangesAsync();
        return booking;
    }

    public async Task<List<BookingModel>> GetPendingBookingsAsync() =>
        (await db.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync()).Select(BookingMapper.ToDomain).ToList();

    public Task<List<Guid>> GetPendingBookingsIdsAsync() =>
        db.Bookings.Where(b => b.Status == BookingStatus.Pending).Select(b => b.Id).ToListAsync();

    public Task<int> CountActiveBookingsByUserIdAsync(Guid userId) =>
        db.Bookings.CountAsync(b => b.UserId == userId && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));

    public void UpdateBooking(BookingModel model)
    {
        var entity = db.Bookings.Local.FirstOrDefault(b => b.Id == model.Id)
            ?? db.Bookings.Find(model.Id)
            ?? throw new InvalidOperationException($"Booking {model.Id} not found");
        entity.Status = model.Status;
        entity.ProcessedAt = model.ProcessedAt;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
