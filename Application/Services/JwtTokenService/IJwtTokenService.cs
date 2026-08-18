using Domain.Models;

namespace Application.Services.JwtTokenService
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserModel user);
    }
}
