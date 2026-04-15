using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.DTOs;

namespace TruePal.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class ApiPostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<ApiPostsController> _logger;

    public ApiPostsController(IPostService postService, ILogger<ApiPostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    /// <summary>
    /// Get a specific post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(int id)
    {
        try
        {
            var result = await _postService.GetPostByIdAsync(id);

            if (!result.IsSuccess)
            {
                return MapError(result.Error!, result.ErrorCode);
            }

            return Ok(PostResponse.FromPost(result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving post {PostId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the post" });
        }
    }

    /// <summary>
    /// Get recent posts (feed)
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentPosts([FromQuery] int count = 10)
    {
        try
        {
            var result = await _postService.GetRecentPostsAsync(count);

            if (!result.IsSuccess)
            {
                return MapError(result.Error!, result.ErrorCode);
            }

            var posts = result.Data!.Select(p => PostResponse.FromPost(p));
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent posts");
            return StatusCode(500, new { error = "An error occurred while retrieving posts" });
        }
    }

    /// <summary>
    /// Get trending posts
    /// </summary>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingPosts([FromQuery] int count = 10)
    {
        try
        {
            var result = await _postService.GetTrendingPostsAsync(count);

            if (!result.IsSuccess)
            {
                return MapError(result.Error!, result.ErrorCode);
            }

            var posts = result.Data!.Select(p => PostResponse.FromPost(p, includeTrendingScore: true));
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trending posts");
            return StatusCode(500, new { error = "An error occurred while retrieving posts" });
        }
    }

    /// <summary>
    /// Get posts by a specific user
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPosts(int userId)
    {
        try
        {
            var result = await _postService.GetUserPostsAsync(userId);

            if (!result.IsSuccess)
            {
                return MapError(result.Error!, result.ErrorCode);
            }

            var posts = result.Data!.Select(p => PostResponse.FromPost(p));
            return Ok(posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving posts for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while retrieving posts" });
        }
    }

    /// <summary>
    /// Create a new post (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
    {
        try
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            var result = await _postService.CreatePostAsync(userId, dto.Content, dto.Location, dto.ImageUrl);

            if (!result.IsSuccess)
            {
                return MapError(result.Error, result.ErrorCode, result.Errors);
            }

            return CreatedAtAction(
                nameof(GetPost),
                new { id = result.Data!.Id },
                PostResponse.FromPost(result.Data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new { error = "An error occurred while creating the post" });
        }
    }

    /// <summary>
    /// Update an existing post (requires authentication and ownership)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto dto)
    {
        try
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            var result = await _postService.UpdatePostAsync(id, userId, dto.Content, dto.Location, dto.ImageUrl);

            if (!result.IsSuccess)
            {
                return MapError(result.Error, result.ErrorCode, result.Errors);
            }

            return Ok(PostResponse.FromPost(result.Data!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the post" });
        }
    }

    /// <summary>
    /// Delete a post (requires authentication and ownership)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        try
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            var result = await _postService.DeletePostAsync(id, userId);

            if (!result.IsSuccess)
            {
                return MapError(result.Error!, result.ErrorCode);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the post" });
        }
    }

    /// <summary>
    /// Increment view count for a post
    /// </summary>
    [HttpPost("{id}/views")]
    public async Task<IActionResult> IncrementViews(int id)
    {
        try
        {
            var result = await _postService.IncrementViewsAsync(id);

            if (!result.IsSuccess)
            {
                return MapError(result.Error!, result.ErrorCode);
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing views for post {PostId}", id);
            return StatusCode(500, new { error = "An error occurred while updating views" });
        }
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out userId);
    }

    private IActionResult MapError(string? error, string? errorCode, List<string>? errors = null)
    {
        if (errors != null && errors.Count > 0)
        {
            return BadRequest(new { errors });
        }

        return errorCode switch
        {
            ErrorCodes.NotFound => NotFound(new { error }),
            ErrorCodes.Forbidden => StatusCode(403, new { error }),
            ErrorCodes.Validation => BadRequest(new { error }),
            _ => BadRequest(new { error })
        };
    }
}
