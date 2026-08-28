using Microsoft.EntityFrameworkCore;
using User.Infrastructure.DataAccess;

namespace User.Presentation;

public static class MigrationExtensions
{
    public static WebApplication ApplyUserMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();
        return app;
    }
}
