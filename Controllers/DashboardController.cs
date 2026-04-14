using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Controllers.Base;
using TruePal.Api.Core.Interfaces;

namespace TruePal.Api.Controllers;

public class DashboardController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<DashboardController> logger) 
        : base(logger, configuration)
    {
        _unitOfWork = unitOfWork;
    }

    public IActionResult Index()
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToActionWithError("Login", "Auth", "Please login to access the dashboard");
        }

        // TODO: Get username from JWT token and load actual dashboard data
        // Sample data for demonstration
        var model = new DashboardViewModel
        {
            Username = "User",
            TotalPosts = 5,
            TotalReach = 1250,
            TotalSaved = 8,
            TotalLikes = 342,
            
            // Sample recent posts from all users (Feed)
            RecentPosts = new List<PostCardViewModel>
            {
                new PostCardViewModel
                {
                    Id = 1,
                    Username = "Maria Santos",
                    UserInitials = "MS",
                    Location = "Vigan, Ilocos Sur",
                    Content = "Colonial architecture and cobblestone streets! Vigan takes you back in time. Don't miss the empanada and bagnet!",
                    ImageUrl = "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?w=400&h=250&fit=crop",
                    LikesCount = 234,
                    CommentsCount = 45,
                    TimeAgo = "2 hours ago",
                    Category = "Culture"
                },
                new PostCardViewModel
                {
                    Id = 2,
                    Username = "Jake Rodriguez",
                    UserInitials = "JR",
                    Location = "Cloud 9, Siargao",
                    Content = "Best waves in the Philippines! Surfing capital for a reason. Also amazing island hopping spots nearby.",
                    ImageUrl = "https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=400&h=250&fit=crop",
                    LikesCount = 456,
                    CommentsCount = 78,
                    TimeAgo = "5 hours ago",
                    Category = "Adventure"
                },
                new PostCardViewModel
                {
                    Id = 3,
                    Username = "Ana Garcia",
                    UserInitials = "AG",
                    Location = "Cebu City",
                    Content = "Lechon capital! The best lechon you'll ever taste is here. Also great beaches nearby in Moalboal and Oslob.",
                    ImageUrl = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&h=250&fit=crop",
                    LikesCount = 189,
                    CommentsCount = 32,
                    TimeAgo = "1 day ago",
                    Category = "Food"
                }
            },
            
            // Sample user's own posts
            YourPosts = new List<PostCardViewModel>
            {
                new PostCardViewModel
                {
                    Id = 101,
                    Username = "User",
                    UserInitials = "U",
                    Location = "Baguio City",
                    Content = "Perfect 18°C weather and fresh strawberries! Session Road and Burnham Park are must-visits.",
                    ImageUrl = "https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400&h=250&fit=crop",
                    LikesCount = 67,
                    CommentsCount = 12,
                    TimeAgo = "3 days ago",
                    Category = "Nature"
                }
            },
            
            // Sample saved places
            SavedPlaces = new List<SavedPlaceViewModel>
            {
                new SavedPlaceViewModel { Location = "El Nido, Palawan", SavedBy = 234 },
                new SavedPlaceViewModel { Location = "Chocolate Hills, Bohol", SavedBy = 456 },
                new SavedPlaceViewModel { Location = "Mayon Volcano, Bicol", SavedBy = 189 }
            }
        };

        return View(model);
    }
}

#region ViewModels

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

#endregion
