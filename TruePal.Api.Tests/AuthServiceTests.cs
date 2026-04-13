using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TruePal.Api.Application.Services;
using TruePal.Api.Infrastructure;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly UnitOfWork _unitOfWork;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _testDb = new TestDbContext();
        _unitOfWork = new UnitOfWork(_testDb.Context);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ThisIsATestKeyThatIsAtLeast32BytesLong!",
                ["Jwt:Issuer"] = "TruePal.Api.Tests",
                ["Jwt:Audience"] = "TruePal.Tests",
                ["Jwt:ExpireMinutes"] = "60"
            })
            .Build();

        var logger = NullLogger<AuthService>.Instance;
        _authService = new AuthService(_unitOfWork, config, logger);
    }

    // --- Registration Tests ---

    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsSuccess()
    {
        var result = await _authService.RegisterAsync("testuser", "test@example.com", "password123");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("testuser", result.Data.Username);
        Assert.Equal("test@example.com", result.Data.Email);
        Assert.True(result.Data.Id > 0);
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_HashesPassword()
    {
        var result = await _authService.RegisterAsync("testuser", "test@example.com", "password123");

        Assert.True(result.IsSuccess);
        Assert.NotEqual("password123", result.Data!.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", result.Data.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        await _authService.RegisterAsync("user1", "same@example.com", "password123");

        var result = await _authService.RegisterAsync("user2", "same@example.com", "password456");

        Assert.False(result.IsSuccess);
        Assert.Equal("User with this email already exists", result.Error);
    }

    [Fact]
    public async Task RegisterAsync_EmptyUsername_ReturnsValidationError()
    {
        var result = await _authService.RegisterAsync("", "test@example.com", "password123");

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_InvalidEmail_ReturnsValidationError()
    {
        var result = await _authService.RegisterAsync("testuser", "not-an-email", "password123");

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_ShortPassword_ReturnsValidationError()
    {
        var result = await _authService.RegisterAsync("testuser", "test@example.com", "12345");

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task RegisterAsync_ShortUsername_ReturnsValidationError()
    {
        var result = await _authService.RegisterAsync("ab", "test@example.com", "password123");

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    // --- Login Tests ---

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        await _authService.RegisterAsync("testuser", "test@example.com", "password123");

        var result = await _authService.LoginAsync("test@example.com", "password123");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        await _authService.RegisterAsync("testuser", "test@example.com", "password123");

        var result = await _authService.LoginAsync("test@example.com", "wrongpassword");

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password", result.Error);
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ReturnsFailure()
    {
        var result = await _authService.LoginAsync("nobody@example.com", "password123");

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password", result.Error);
    }

    [Fact]
    public async Task LoginAsync_EmptyEmail_ReturnsValidationError()
    {
        var result = await _authService.LoginAsync("", "password123");

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ReturnsValidationError()
    {
        var result = await _authService.LoginAsync("test@example.com", "");

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    // --- Full Flow ---

    [Fact]
    public async Task RegisterThenLogin_ReturnsValidToken()
    {
        await _authService.RegisterAsync("flowuser", "flow@example.com", "password123");

        var loginResult = await _authService.LoginAsync("flow@example.com", "password123");

        Assert.True(loginResult.IsSuccess);

        // Verify the token is a valid JWT (3 base64 segments separated by dots)
        var parts = loginResult.Data!.Split('.');
        Assert.Equal(3, parts.Length);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _testDb.Dispose();
    }
}
