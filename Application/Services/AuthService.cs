using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Core.Validators;
using TruePal.Api.Models;

namespace TruePal.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration config, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _config = config;
        _logger = logger;
    }

    public async Task<Result<User>> RegisterAsync(string username, string email, string password)
    {
        try
        {
            // Validate input
            var validationErrors = ValidationHelper.ValidateRegister(username, email, password);
            if (validationErrors.Any())
            {
                return Result<User>.Failure(validationErrors);
            }

            // Check if email already exists
            var emailExists = await _unitOfWork.Users.EmailExistsAsync(email);
            if (emailExists)
            {
                return Result<User>.Failure("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {Email} registered successfully", email);

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for {Email}", email);
            return Result<User>.Failure("An error occurred during registration");
        }
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        try
        {
            // Validate input
            var validationErrors = ValidationHelper.ValidateLogin(email, password);
            if (validationErrors.Any())
            {
                return Result<string>.Failure(validationErrors);
            }

            // Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
                return Result<string>.Failure("Invalid email or password");
            }

            // Verify password
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", email);
                return Result<string>.Failure("Invalid email or password");
            }

            // Generate JWT token
            var token = GenerateToken(user);

            _logger.LogInformation("User {Email} logged in successfully", email);

            return Result<string>.Success(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", email);
            return Result<string>.Failure("An error occurred during login");
        }
    }

    private string GenerateToken(User user)
    {
        var jwt = _config.GetSection("Jwt");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
