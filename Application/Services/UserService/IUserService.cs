using Application.DTO.Users;

namespace Application.Services.UserService
{
    public interface IUserService
    {
        Task RegisterAsync(RegisterUserRequestDto request);
        Task<AuthResultDto> LoginAsync(LoginRequestDto request);
    }
}
