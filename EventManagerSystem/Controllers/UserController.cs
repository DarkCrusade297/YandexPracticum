using Application.DTO.Users;
using Application.Services.UserService;
using Microsoft.AspNetCore.Mvc;

namespace EventManagerSystem.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
        {
            await _userService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _userService.LoginAsync(request);
            return Ok(result);
        }
    }
}
