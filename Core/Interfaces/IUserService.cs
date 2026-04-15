using TruePal.Api.Models;

namespace TruePal.Api.Core.Interfaces;

/// <summary>
/// Service interface for user profile management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <returns>Result containing the user if found, or NotFound error if user doesn't exist</returns>
    Task<Result<User>> GetUserByIdAsync(int userId);

    /// <summary>
    /// Retrieves a user by their email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>Result containing the user if found, or NotFound error if user doesn't exist</returns>
    Task<Result<User>> GetUserByEmailAsync(string email);

    /// <summary>
    /// Retrieves a user by their username
    /// </summary>
    /// <param name="username">The username of the user</param>
    /// <returns>Result containing the user if found, or NotFound error if user doesn't exist</returns>
    Task<Result<User>> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Updates a user's profile information including bio and profile picture URL
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="bio">The user's biography text (max 500 characters), or null to leave unchanged</param>
    /// <param name="profilePictureUrl">The URL to the user's profile picture (max 500 characters), or null to leave unchanged</param>
    /// <returns>Result containing the updated user, validation errors, or NotFound error if user doesn't exist</returns>
    Task<Result<User>> UpdateProfileAsync(int userId, string? bio, string? profilePictureUrl);

    /// <summary>
    /// Updates a user's email address with validation and duplicate check
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="newEmail">The new email address</param>
    /// <returns>Result indicating success, validation errors, duplicate email error, or NotFound error</returns>
    Task<Result<bool>> UpdateEmailAsync(int userId, string newEmail);

    /// <summary>
    /// Updates a user's password after verifying the current password
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="currentPassword">The user's current password for verification</param>
    /// <param name="newPassword">The new password (min 6 characters)</param>
    /// <returns>Result indicating success, validation errors, incorrect password error, or NotFound error</returns>
    Task<Result<bool>> UpdatePasswordAsync(int userId, string currentPassword, string newPassword);
}
