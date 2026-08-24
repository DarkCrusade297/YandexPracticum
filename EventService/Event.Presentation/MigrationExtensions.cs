using Event.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Event.Presentation;

public static class MigrationExtensions
{
    public static WebApplication ApplyEventMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<EventDbContext>().Database.Migrate();
        return app;
    }
}
