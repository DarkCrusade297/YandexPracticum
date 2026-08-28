namespace User.Application.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
