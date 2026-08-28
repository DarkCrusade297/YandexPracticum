using Microsoft.EntityFrameworkCore;
using User.Application.Common.Interfaces;
using User.Domain.Models;
using User.Infrastructure.DataAccess;
using User.Infrastructure.Mapper;

namespace User.Infrastructure.Repositories;

public class UserRepository(UserDbContext db) : IUserRepository
{
    public async Task<UserModel> CreateUserAsync(UserModel user)
    {
        await db.Users.AddAsync(UserMapper.ToEntity(user));
        return user;
    }

    public async Task<UserModel?> GetUserByIdAsync(Guid id)
    {
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<UserModel?> GetUserByLoginAsync(string login)
    {
        var entity = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == login);
        return entity is null ? null : UserMapper.ToDomain(entity);
    }

    public void UpdateUser(UserModel user) => db.Users.Update(UserMapper.ToEntity(user));
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
