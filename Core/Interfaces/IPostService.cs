using TruePal.Api.Models;

namespace TruePal.Api.Core.Interfaces;

public interface IPostService
{
    Task<Result<Post>> CreatePostAsync(int userId, string content, string? location, string? imageUrl);
    Task<Result<Post>> GetPostByIdAsync(int id);
    Task<Result<IEnumerable<Post>>> GetUserPostsAsync(int userId);
    Task<Result<IEnumerable<Post>>> GetRecentPostsAsync(int count);
    Task<Result<IEnumerable<Post>>> GetTrendingPostsAsync(int count);
    Task<Result<Post>> UpdatePostAsync(int postId, int userId, string content, string? location, string? imageUrl);
    Task<Result<bool>> DeletePostAsync(int postId, int userId);
    Task<Result<bool>> IncrementViewsAsync(int postId);
}
