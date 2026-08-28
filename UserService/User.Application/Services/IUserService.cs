using User.Application.DTO;

namespace User.Application.Services;

public interface IUserService
{
    Task RegisterAsync(RegisterUserRequestDto request);
    Task<AuthResultDto> LoginAsync(LoginRequestDto request);
}
