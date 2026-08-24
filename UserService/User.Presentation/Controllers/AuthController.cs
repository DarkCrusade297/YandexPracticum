using Microsoft.AspNetCore.Mvc;
using User.Application.DTO;
using User.Application.Services;

namespace User.Presentation.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequestDto request)
    {
        await userService.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginRequestDto request) =>
        Ok(await userService.LoginAsync(request));
}
