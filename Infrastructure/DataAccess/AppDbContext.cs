using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<EventEntity> Events => Set<EventEntity>();
        public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
