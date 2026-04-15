using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruePal.Api.Models;

public class Post
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;

    public int ViewsCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Foreign key
    [Required]
    public int UserId { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    // Computed property for trending score (must match PostRepository.GetTrendingPostsAsync ordering)
    [NotMapped]
    public double TrendingScore =>
        (LikesCount * 2) + (CommentsCount * 3) + (ViewsCount * 0.5);
}
