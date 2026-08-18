namespace Application.DTO.Users
{
    public record RegisterUserRequestDto(string Login, string Password);

    public record LoginRequestDto(string Login, string Password);

    public record AuthResultDto(string Token);
}

