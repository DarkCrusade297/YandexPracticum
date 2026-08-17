using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<UserModel> CreateUserAsync(UserModel user);
        void UpdateUser(UserModel user);
        Task SaveChangesAsync();
    }
}
