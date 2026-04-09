using Microsoft.AspNetCore.Mvc;
using TruePal.Api.DTOs;
using TruePal.Api.Services;

namespace TruePal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = await _auth.Register(dto.Username, dto.Email, dto.Password);
        if (user == null) return BadRequest("User Already Exists");
        return Ok(user);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _auth.Login(dto.Email, dto.Password);
        if (token == null) return Unauthorized("Invalid credentials");
        return Ok(new { token });
    }
}