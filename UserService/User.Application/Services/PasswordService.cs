using System.Security.Cryptography;
using System.Text;

namespace User.Application.Services;

public class PasswordService : IPasswordService
{
    public string Hash(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    public bool Verify(string password, string passwordHash) => Hash(password) == passwordHash;
}
