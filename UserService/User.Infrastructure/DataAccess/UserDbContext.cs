using Microsoft.EntityFrameworkCore;
using User.Infrastructure.Entities;

namespace User.Infrastructure.DataAccess;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
}
