using User.Domain.Models;
using User.Infrastructure.Entities;

namespace User.Infrastructure.Mapper;

public static class UserMapper
{
    public static UserEntity ToEntity(UserModel model) => new()
    {
        Id = model.Id, Login = model.Login, PasswordHash = model.PasswordHash, Role = model.Role
    };

    public static UserModel ToDomain(UserEntity entity) =>
        new(entity.Id, entity.Login, entity.PasswordHash, entity.Role);
}
