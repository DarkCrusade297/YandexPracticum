using Booking.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Booking.Presentation;

public static class MigrationExtensions
{
    public static WebApplication ApplyBookingMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<BookingDbContext>().Database.Migrate();
        return app;
    }
}
