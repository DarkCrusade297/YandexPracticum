using User.Application.Common.Interfaces;
using User.Application.DTO;
using User.Domain.Exceptions;
using User.Domain.Models;

namespace User.Application.Services;

public class UserService(IUserRepository userRepository, IPasswordService passwordService, IJwtTokenService jwtTokenService) : IUserService
{
    public async Task RegisterAsync(RegisterUserRequestDto request)
    {
        if (await userRepository.GetUserByLoginAsync(request.Login) is not null)
            throw new UserAlreadyExistsException(request.Login);
        var user = new UserModel(request.Login, passwordService.Hash(request.Password), request.Role);
        await userRepository.CreateUserAsync(user);
        await userRepository.SaveChangesAsync();
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await userRepository.GetUserByLoginAsync(request.Login);
        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();
        return new AuthResultDto(jwtTokenService.GenerateToken(user));
    }
}
