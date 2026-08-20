using Domain.Enums;

namespace Application.DTO.Users
{
    public record RegisterUserRequestDto(string Login, string Password, UserRoles role);

    public record LoginRequestDto(string Login, string Password);

    public record AuthResultDto(string Token);
}

