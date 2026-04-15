using System.ComponentModel.DataAnnotations;

namespace TruePal.Api.ViewModels;

public class DashboardViewModel
{
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Total Posts")]
    public int TotalPosts { get; set; }

    [Display(Name = "Total Reach")]
    public int TotalReach { get; set; }

    [Display(Name = "Total Saved")]
    public int TotalSaved { get; set; }

    [Display(Name = "Total Likes")]
    public int TotalLikes { get; set; }

    public List<PostCardViewModel> RecentPosts { get; set; } = new();
    public List<PostCardViewModel> YourPosts { get; set; } = new();
    public List<SavedPlaceViewModel> SavedPlaces { get; set; } = new();
}

public class PostCardViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserInitials { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class SavedPlaceViewModel
{
    public string Location { get; set; } = string.Empty;
    public int SavedBy { get; set; }
}

public class ProfileViewModel
{
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }

    public int TotalPosts { get; set; }

    public List<PostCardViewModel> YourPosts { get; set; } = new();
}
