using TruePal.Api.Core.Interfaces;
using TruePal.Api.Models;

namespace TruePal.Api.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Result containing the user if found, or NotFound error if user doesn't exist</returns>
    public async Task<Result<User>> GetUserByIdAsync(int userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            return Result<User>.Failure("User not found", ErrorCodes.NotFound);
        }

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Retrieves a user by their email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>Result containing the user if found, or NotFound error if user doesn't exist</returns>
    public async Task<Result<User>> GetUserByEmailAsync(string email)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        
        if (user == null)
        {
            return Result<User>.Failure("User not found", ErrorCodes.NotFound);
        }

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Retrieves a user by their username
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <returns>Result containing the user if found, or NotFound error if user doesn't exist</returns>
    public async Task<Result<User>> GetUserByUsernameAsync(string username)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(username);
        
        if (user == null)
        {
            return Result<User>.Failure("User not found", ErrorCodes.NotFound);
        }

        return Result<User>.Success(user);
    }

    /// <summary>
    /// Updates a user's profile information including bio and profile picture URL
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="bio">The user's biography text (max 500 characters), or null to leave unchanged</param>
    /// <param name="profilePictureUrl">The URL to the user's profile picture (max 500 characters), or null to leave unchanged</param>
    /// <returns>Result containing the updated user, validation errors, or NotFound error if user doesn't exist</returns>
    public async Task<Result<User>> UpdateProfileAsync(int userId, string? bio, string? profilePictureUrl)
    {
        var validationErrors = new List<string>();

        if (bio != null && bio.Length > 500)
        {
            validationErrors.Add("Bio must not exceed 500 characters");
        }

        if (profilePictureUrl != null && profilePictureUrl.Length > 500)
        {
            validationErrors.Add("Profile picture URL must not exceed 500 characters");
        }

        if (validationErrors.Any())
        {
            return Result<User>.Failure(validationErrors);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            return Result<User>.Failure("User not found", ErrorCodes.NotFound);
        }

        // Update fields
        user.Bio = bio;
        user.ProfilePictureUrl = profilePictureUrl;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Profile updated for user {UserId}", userId);
        return Result<User>.Success(user);
    }

    /// <summary>
    /// Updates a user's email address with validation and duplicate check
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="newEmail">The new email address</param>
    /// <returns>Result indicating success, validation errors, duplicate email error, or NotFound error</returns>
    public async Task<Result<bool>> UpdateEmailAsync(int userId, string newEmail)
    {
        // Validation
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(newEmail))
        {
            validationErrors.Add("Email is required");
        }
        else if (!IsValidEmail(newEmail))
        {
            validationErrors.Add("Invalid email format");
        }

        if (validationErrors.Any())
        {
            return Result<bool>.Failure(validationErrors);
        }

        // Check if email is already in use
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(newEmail);
        if (existingUser != null && existingUser.Id != userId)
        {
            return Result<bool>.Failure("Email is already in use");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            return Result<bool>.Failure("User not found", ErrorCodes.NotFound);
        }

        user.Email = newEmail;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Email updated for user {UserId}", userId);
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Updates a user's password after verifying the current password
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="currentPassword">The user's current password for verification</param>
    /// <param name="newPassword">The new password (min 6 characters)</param>
    /// <returns>Result indicating success, validation errors, incorrect password error, or NotFound error</returns>
    public async Task<Result<bool>> UpdatePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        // Validation
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            validationErrors.Add("Current password is required");
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            validationErrors.Add("New password is required");
        }
        else if (newPassword.Length < 6)
        {
            validationErrors.Add("New password must be at least 6 characters long");
        }

        if (validationErrors.Any())
        {
            return Result<bool>.Failure(validationErrors);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        
        if (user == null)
        {
            return Result<bool>.Failure("User not found", ErrorCodes.NotFound);
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return Result<bool>.Failure("Current password is incorrect");
        }

        // Hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Password updated for user {UserId}", userId);
        return Result<bool>.Success(true);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
