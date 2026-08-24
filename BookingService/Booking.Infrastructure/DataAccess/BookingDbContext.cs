using Booking.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.DataAccess;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
}
