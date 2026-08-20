using Domain.Models;
using Infrastructure.Entities;

namespace Infrastructure.Mapper
{
    public class UserMapper
    {
        public static UserEntity ToEntity(UserModel userDomain)
        {
            return new UserEntity
            {
                Id = userDomain.Id,
                Login = userDomain.Login,
                PasswordHash = userDomain.PasswordHash,
                Role = userDomain.Role
            };
        }

        public static UserModel ToDomain(UserEntity entity)
        {
            var model = new UserModel(entity.Id, entity.Login, entity.PasswordHash, entity.Role);
            return model;
        }
    }
}
