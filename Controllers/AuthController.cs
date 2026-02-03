using Microsoft.AspNetCore.Mvc;
using QAWebApp.DTOs;
using QAWebApp.Services.Interfaces;

namespace QAWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IJwtService jwtService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.RegisterAsync(dto);

        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        var token = _jwtService.GenerateToken(result.User!);

        return Ok(new
        {
            message = result.Message,
            token,
            user = new
            {
                id = result.User!.Id,
                username = result.User.Username,
                email = result.User.Email
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _userService.LoginAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        var token = _jwtService.GenerateToken(result.User!);

        return Ok(new
        {
            message = result.Message,
            token,
            user = new
            {
                id = result.User!.Id,
                username = result.User.Username,
                email = result.User.Email
            }
        });
    }
}
