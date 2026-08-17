using Application.Common.Interfaces;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

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
            var _user = UserMapper.ToEntity(user);
            await _db.Users.AddAsync(_user);
            return UserMapper.ToDomain(_user);
        }

        public void UpdateUser(UserModel ev)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
