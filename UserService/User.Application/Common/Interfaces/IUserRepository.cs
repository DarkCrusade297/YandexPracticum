using User.Domain.Models;

namespace User.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<UserModel> CreateUserAsync(UserModel user);
    Task<UserModel?> GetUserByIdAsync(Guid id);
    Task<UserModel?> GetUserByLoginAsync(string login);
    void UpdateUser(UserModel user);
    Task SaveChangesAsync();
}
