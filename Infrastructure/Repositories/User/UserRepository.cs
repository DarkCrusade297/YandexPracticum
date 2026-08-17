using Application.Common.Interfaces;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        public Task<UserModel> CreateUserAsync(UserModel user)
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(UserModel ev)
        {
            throw new NotImplementedException();
        }
    }
}
