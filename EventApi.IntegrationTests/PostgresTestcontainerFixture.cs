using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManagerSystem.Tests;

public sealed class PostgresTestcontainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSql = new PostgreSqlBuilder("postgres:16-alpine").Build();


    public string ConnectionString => postgreSql.GetConnectionString();

    public async Task InitializeAsync()
    {
        await postgreSql.StartAsync();
        await using var context = CreateDbContext();
        await context.Database.MigrateAsync();

    }

    public async Task DisposeAsync()
    {
        await postgreSql.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var db = CreateDbContext();

        db.Events.RemoveRange(db.Events);
        db.Bookings.RemoveRange(db.Bookings);

        await db.SaveChangesAsync();
    }
}
