using System.Security.Cryptography;
using System.Text;

namespace Application.Services.PasswordService
{
    public class PasswordService : IPasswordService
    {
        public string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public bool Verify(string password, string passwordHash)
        {
            return Hash(password) == passwordHash;
        }
    }
}
