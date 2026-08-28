using Event.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Event.Infrastructure.DataAccess;

public sealed class EventDbContext(DbContextOptions<EventDbContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events => Set<EventEntity>();
    public DbSet<ProcessedBookingEntity> ProcessedBookings => Set<ProcessedBookingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
}
