using Application.Common.Interfaces;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Mapper;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserModel> CreateUserAsync(UserModel user)
        {
            var entity = UserMapper.ToEntity(user);
            await _db.Users.AddAsync(entity);
            return UserMapper.ToDomain(entity);
        }

        public async Task<UserModel?> GetUserByIdAsync(Guid id)
        {
            var entity = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return entity is null ? null : UserMapper.ToDomain(entity);
        }

        public async Task<UserModel?> GetUserByLoginAsync(string login)
        {
            var entity = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == login);

            return entity is null ? null : UserMapper.ToDomain(entity);
        }

        public void UpdateUser(UserModel user)
        {
            var entity = UserMapper.ToEntity(user);
            _db.Users.Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
