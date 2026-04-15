using System.ComponentModel.DataAnnotations;

namespace TruePal.Api.DTOs;

public class CreatePostDto
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 500 characters")]
    public string Content { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Location must be 200 characters or less")]
    public string? Location { get; set; }

    [StringLength(500, ErrorMessage = "Image URL must be 500 characters or less")]
    public string? ImageUrl { get; set; }
}

public class UpdatePostDto
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 500 characters")]
    public string Content { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Location must be 200 characters or less")]
    public string? Location { get; set; }

    [StringLength(500, ErrorMessage = "Image URL must be 500 characters or less")]
    public string? ImageUrl { get; set; }
}
