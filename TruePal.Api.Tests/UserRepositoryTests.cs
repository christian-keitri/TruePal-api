using TruePal.Api.Infrastructure.Repositories;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;

namespace TruePal.Api.Tests;

public class UserRepositoryTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _testDb = new TestDbContext();
        _repository = new UserRepository(_testDb.Context);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Users.AddAsync(user);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync("test@example.com");

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        var result = await _repository.GetByEmailAsync("nobody@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUsername_ReturnsUser()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Users.AddAsync(user);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetByUsernameAsync("testuser");

        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_NonExistentUsername_ReturnsNull()
    {
        var result = await _repository.GetByUsernameAsync("nobody");

        Assert.Null(result);
    }

    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Users.AddAsync(user);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.EmailExistsAsync("test@example.com");

        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        var result = await _repository.EmailExistsAsync("nobody@example.com");

        Assert.False(result);
    }

    [Fact]
    public async Task AddAsync_NewUser_PersistsToDatabase()
    {
        var user = new User
        {
            Username = "newuser",
            Email = "new@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user);
        await _testDb.Context.SaveChangesAsync();

        var saved = await _repository.GetByEmailAsync("new@example.com");
        Assert.NotNull(saved);
        Assert.Equal("newuser", saved.Username);
        Assert.True(saved.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        await _testDb.Context.Users.AddAsync(user);
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetAllAsync_MultipleUsers_ReturnsAll()
    {
        await _testDb.Context.Users.AddRangeAsync(
            new User { Username = "user1", Email = "u1@example.com", PasswordHash = "h1" },
            new User { Username = "user2", Email = "u2@example.com", PasswordHash = "h2" }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    public void Dispose() => _testDb.Dispose();
}
