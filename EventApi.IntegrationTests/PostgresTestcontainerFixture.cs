using Booking.Infrastructure.DataAccess;
using Event.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests;

public sealed class PostgresTestcontainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _eventPostgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("events_tests")
        .Build();
    private readonly PostgreSqlContainer _bookingPostgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("bookings_tests")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_eventPostgres.StartAsync(), _bookingPostgres.StartAsync());
        await using var eventDb = CreateEventDbContext();
        await using var bookingDb = CreateBookingDbContext();
        await Task.WhenAll(eventDb.Database.MigrateAsync(), bookingDb.Database.MigrateAsync());
    }

    public async Task DisposeAsync()
    {
        await _eventPostgres.DisposeAsync();
        await _bookingPostgres.DisposeAsync();
    }

    public EventDbContext CreateEventDbContext()
    {
        var options = new DbContextOptionsBuilder<EventDbContext>()
            .UseNpgsql(_eventPostgres.GetConnectionString())
            .Options;
        return new EventDbContext(options);
    }

    public BookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(_bookingPostgres.GetConnectionString())
            .Options;
        return new BookingDbContext(options);
    }

    public async Task ResetEventsAsync()
    {
        await using var db = CreateEventDbContext();
        await db.ProcessedBookings.ExecuteDeleteAsync();
        await db.Events.ExecuteDeleteAsync();
    }

    public async Task ResetBookingsAsync()
    {
        await using var db = CreateBookingDbContext();
        await db.Bookings.ExecuteDeleteAsync();
    }
}
