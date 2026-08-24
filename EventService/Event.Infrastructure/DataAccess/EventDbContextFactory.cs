using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Event.Infrastructure.DataAccess;

public sealed class EventDbContextFactory : IDesignTimeDbContextFactory<EventDbContext>
{
    public EventDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=event_db;Username=postgres;Password=postgres")
            .Options;
        return new EventDbContext(options);
    }
}
