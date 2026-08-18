using Application.Common.Interfaces;
using Application.DTO.Users;
using Application.Services.JwtTokenService;
using Application.Services.PasswordService;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using System.Security.Authentication;

namespace Application.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;

        public UserService(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task RegisterAsync(RegisterUserRequestDto request)
        {
            var existingUser = await _userRepository.GetUserByLoginAsync(request.Login);
            if (existingUser is not null)
            {
                throw new UserAlreadyExistsException(request.Login);
            }

            var passwordHash = _passwordService.Hash(request.Password);
            var user = new UserModel(request.Login, passwordHash, UserRoles.User);

            await _userRepository.CreateUserAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetUserByLoginAsync(request.Login);
            if (user is null)
            {
                throw new InvalidCredentialsException();
            }

            var isPasswordValid = _passwordService.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new InvalidCredentialsException();
            }

            var token = _jwtTokenService.GenerateToken(user);
            return new AuthResultDto(token);
        }
    }
}
