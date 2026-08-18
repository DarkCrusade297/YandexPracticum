using Domain.Enums;

namespace Domain.Models
{
    public class UserModel
    {
        public Guid Id { get; private set; }
        public string Login { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRoles Role { get; private set; }

        public UserModel(string login, string passwordHash, UserRoles role)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentException("Login cannot be empty", nameof(login));
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password cannot be empty", nameof(passwordHash));
            }
            
            Id = Guid.NewGuid();
            Login = login;
            PasswordHash = passwordHash;
            Role = role;
        }

        public UserModel(Guid id, string login, string passwordHash, UserRoles role)
        {
            Id = id;
            Login = login;
            PasswordHash = passwordHash;
            Role = role;
        }
    }
}
