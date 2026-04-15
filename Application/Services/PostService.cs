using TruePal.Api.Core.Interfaces;
using TruePal.Api.Models;

namespace TruePal.Api.Application.Services;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PostService> _logger;

    public PostService(IUnitOfWork unitOfWork, ILogger<PostService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Post>> CreatePostAsync(int userId, string content, string? location, string? imageUrl)
    {
        var validationErrors = ValidatePostInput(content, location, imageUrl);
        if (validationErrors.Count > 0)
        {
            return Result<Post>.Failure(validationErrors);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<Post>.Failure("User not found", ErrorCodes.NotFound);
        }

        var post = new Post
        {
            UserId = userId,
            Content = content,
            Location = location,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Posts.AddAsync(post);
        await _unitOfWork.CompleteAsync();

        var createdPost = await _unitOfWork.Posts.GetByIdWithUserAsync(post.Id);

        _logger.LogInformation("Post {PostId} created by user {UserId}", post.Id, userId);

        return Result<Post>.Success(createdPost!);
    }

    public async Task<Result<Post>> GetPostByIdAsync(int id)
    {
        var post = await _unitOfWork.Posts.GetByIdWithUserAsync(id);

        if (post == null)
        {
            return Result<Post>.Failure("Post not found", ErrorCodes.NotFound);
        }

        return Result<Post>.Success(post);
    }

    public async Task<Result<IEnumerable<Post>>> GetUserPostsAsync(int userId)
    {
        var posts = await _unitOfWork.Posts.GetByUserIdAsync(userId);
        return Result<IEnumerable<Post>>.Success(posts);
    }

    public async Task<Result<IEnumerable<Post>>> GetRecentPostsAsync(int count)
    {
        if (count <= 0 || count > 100)
        {
            return Result<IEnumerable<Post>>.Failure("Count must be between 1 and 100", ErrorCodes.Validation);
        }

        var posts = await _unitOfWork.Posts.GetRecentPostsAsync(count);
        return Result<IEnumerable<Post>>.Success(posts);
    }

    public async Task<Result<IEnumerable<Post>>> GetTrendingPostsAsync(int count)
    {
        if (count <= 0 || count > 100)
        {
            return Result<IEnumerable<Post>>.Failure("Count must be between 1 and 100", ErrorCodes.Validation);
        }

        var posts = await _unitOfWork.Posts.GetTrendingPostsAsync(count);
        return Result<IEnumerable<Post>>.Success(posts);
    }

    public async Task<Result<Post>> UpdatePostAsync(int postId, int userId, string content, string? location, string? imageUrl)
    {
        var validationErrors = ValidatePostInput(content, location, imageUrl);
        if (validationErrors.Count > 0)
        {
            return Result<Post>.Failure(validationErrors);
        }

        var post = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (post == null)
        {
            return Result<Post>.Failure("Post not found", ErrorCodes.NotFound);
        }

        if (post.UserId != userId)
        {
            return Result<Post>.Failure("You are not authorized to update this post", ErrorCodes.Forbidden);
        }

        post.Content = content;
        post.Location = location;
        post.ImageUrl = imageUrl;
        post.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Posts.Update(post);
        await _unitOfWork.CompleteAsync();

        var updatedPost = await _unitOfWork.Posts.GetByIdWithUserAsync(postId);

        _logger.LogInformation("Post {PostId} updated by user {UserId}", postId, userId);

        return Result<Post>.Success(updatedPost!);
    }

    public async Task<Result<bool>> DeletePostAsync(int postId, int userId)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (post == null)
        {
            return Result<bool>.Failure("Post not found", ErrorCodes.NotFound);
        }

        if (post.UserId != userId)
        {
            return Result<bool>.Failure("You are not authorized to delete this post", ErrorCodes.Forbidden);
        }

        _unitOfWork.Posts.Delete(post);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Post {PostId} deleted by user {UserId}", postId, userId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> IncrementViewsAsync(int postId)
    {
        var post = await _unitOfWork.Posts.GetByIdAsync(postId);
        if (post == null)
        {
            return Result<bool>.Failure("Post not found", ErrorCodes.NotFound);
        }

        post.ViewsCount++;
        _unitOfWork.Posts.Update(post);
        await _unitOfWork.CompleteAsync();

        return Result<bool>.Success(true);
    }

    private static List<string> ValidatePostInput(string content, string? location, string? imageUrl)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(content))
            errors.Add("Post content is required");
        else if (content.Length > 500)
            errors.Add("Post content must be 500 characters or less");

        if (location != null && location.Length > 200)
            errors.Add("Location must be 200 characters or less");

        if (imageUrl != null && imageUrl.Length > 500)
            errors.Add("Image URL must be 500 characters or less");

        return errors;
    }
}
