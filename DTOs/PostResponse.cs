using TruePal.Api.Models;

namespace TruePal.Api.DTOs;

public class PostResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ImageUrl { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public int ViewsCount { get; set; }
    public double? TrendingScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public PostUserResponse? User { get; set; }

    public static PostResponse FromPost(Post post, bool includeTrendingScore = false)
    {
        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            Location = post.Location,
            ImageUrl = post.ImageUrl,
            LikesCount = post.LikesCount,
            CommentsCount = post.CommentsCount,
            ViewsCount = post.ViewsCount,
            TrendingScore = includeTrendingScore ? post.TrendingScore : null,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            User = post.User != null ? PostUserResponse.FromUser(post.User) : null
        };
    }
}

public class PostUserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    public static PostUserResponse FromUser(User user)
    {
        return new PostUserResponse
        {
            Id = user.Id,
            Username = user.Username
        };
    }
}
