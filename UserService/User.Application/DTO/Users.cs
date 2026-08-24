using User.Domain.Enums;

namespace User.Application.DTO;

public record RegisterUserRequestDto(string Login, string Password, UserRoles Role);
public record LoginRequestDto(string Login, string Password);
public record AuthResultDto(string Token);
