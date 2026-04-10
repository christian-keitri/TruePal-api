using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.DTOs;

namespace TruePal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto.Username, dto.Email, dto.Password);
        
        if (!result.IsSuccess)
        {
            if (result.Errors.Any())
                return BadRequest(new { errors = result.Errors });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { 
            id = result.Data!.Id, 
            username = result.Data.Username, 
            email = result.Data.Email 
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto.Email, dto.Password);
        
        if (!result.IsSuccess)
        {
            if (result.Errors.Any())
                return Unauthorized(new { errors = result.Errors });
            return Unauthorized(new { error = result.Error });
        }

        return Ok(new { token = result.Data });
    }
}