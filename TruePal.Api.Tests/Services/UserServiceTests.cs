using Microsoft.Extensions.Logging.Abstractions;
using TruePal.Api.Application.Services;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Models;
using TruePal.Api.Tests.Helpers;
using FluentAssertions;

namespace TruePal.Api.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly TestDbContext _testDb;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _testDb = new TestDbContext();
        _unitOfWork = new Infrastructure.UnitOfWork(_testDb.Context);
        _userService = new UserService(_unitOfWork, NullLogger<UserService>.Instance);
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _testDb?.Dispose();
    }

    #region GetUserById Tests

    [Fact]
    public async Task GetUserByIdAsync_ExistingUser_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.GetUserByIdAsync(user.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUserByIdAsync_NonExistentUser_ReturnsNotFound()
    {
        // Act
        var result = await _userService.GetUserByIdAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
        result.Error.Should().Be("User not found");
    }

    #endregion

    #region GetUserByEmail Tests

    [Fact]
    public async Task GetUserByEmailAsync_ExistingUser_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.GetUserByEmailAsync("test@example.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUserByEmailAsync_NonExistentUser_ReturnsNotFound()
    {
        // Act
        var result = await _userService.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
    }

    #endregion

    #region UpdateProfile Tests

    [Fact]
    public async Task UpdateProfileAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateProfileAsync(user.Id, "My bio", "/uploads/pic.jpg");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Bio.Should().Be("My bio");
        result.Data.ProfilePictureUrl.Should().Be("/uploads/pic.jpg");
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsNotFound()
    {
        // Act
        var result = await _userService.UpdateProfileAsync(999, "Bio", null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.NotFound);
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateProfileAsync_WithBioOnly_UpdatesBio()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateProfileAsync(user.Id, "Updated bio", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Bio.Should().Be("Updated bio");
        result.Data.ProfilePictureUrl.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_WithProfilePictureOnly_UpdatesUrl()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateProfileAsync(user.Id, null, "/uploads/new.jpg");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.ProfilePictureUrl.Should().Be("/uploads/new.jpg");
        result.Data.Bio.Should().BeNull();
    }

    [Fact]
    public async Task UpdateProfileAsync_BioExceeds500Chars_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var longBio = new string('a', 501);

        // Act
        var result = await _userService.UpdateProfileAsync(user.Id, longBio, null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Bio must not exceed 500 characters");
    }

    [Fact]
    public async Task UpdateProfileAsync_ProfilePictureUrlExceeds500Chars_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var longUrl = new string('a', 501);

        // Act
        var result = await _userService.UpdateProfileAsync(user.Id, null, longUrl);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Profile picture URL must not exceed 500 characters");
    }

    #endregion

    #region UpdateEmail Tests

    [Fact]
    public async Task UpdateEmailAsync_ValidEmail_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "old@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateEmailAsync(user.Id, "new@example.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
        updatedUser!.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task UpdateEmailAsync_EmailAlreadyInUse_ReturnsError()
    {
        // Arrange
        var user1 = new User
        {
            Username = "user1",
            Email = "user1@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        var user2 = new User
        {
            Username = "user2",
            Email = "user2@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user1);
        await _unitOfWork.Users.AddAsync(user2);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateEmailAsync(user1.Id, "user2@example.com");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Email is already in use");
    }

    [Fact]
    public async Task UpdateEmailAsync_InvalidFormat_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateEmailAsync(user.Id, "invalid-email");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid email format");
    }

    [Fact]
    public async Task UpdateEmailAsync_EmptyEmail_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdateEmailAsync(user.Id, "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Email is required");
    }

    #endregion

    #region UpdatePassword Tests

    [Fact]
    public async Task UpdatePasswordAsync_ValidPassword_ReturnsSuccess()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdatePasswordAsync(user.Id, "oldpassword", "newpassword123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
        BCrypt.Net.BCrypt.Verify("newpassword123", updatedUser!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePasswordAsync_WrongCurrentPassword_ReturnsError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdatePasswordAsync(user.Id, "wrongpassword", "newpassword123");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Current password is incorrect");
    }

    [Fact]
    public async Task UpdatePasswordAsync_PasswordTooShort_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdatePasswordAsync(user.Id, "oldpassword", "123");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("New password must be at least 6 characters long");
    }

    [Fact]
    public async Task UpdatePasswordAsync_EmptyCurrentPassword_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdatePasswordAsync(user.Id, "", "newpassword");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Current password is required");
    }

    [Fact]
    public async Task UpdatePasswordAsync_EmptyNewPassword_ReturnsValidationError()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        // Act
        var result = await _userService.UpdatePasswordAsync(user.Id, "oldpassword", "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("New password is required");
    }

    #endregion
}
