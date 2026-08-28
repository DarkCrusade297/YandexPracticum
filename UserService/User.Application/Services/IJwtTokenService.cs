using User.Domain.Models;

namespace User.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(UserModel user);
}
