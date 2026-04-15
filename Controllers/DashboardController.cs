using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Controllers.Base;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.ViewModels;

namespace TruePal.Api.Controllers;

public class DashboardController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostService _postService;

    public DashboardController(
        IUnitOfWork unitOfWork,
        IPostService postService,
        IConfiguration configuration, 
        ILogger<DashboardController> logger) 
        : base(logger, configuration)
    {
        _unitOfWork = unitOfWork;
        _postService = postService;
    }

    public async Task<IActionResult> Index()
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToActionWithError("Login", "Auth", "Please login to access the dashboard");
        }

        try
        {
            // Get user ID from JWT token
            var userId = GetCurrentUserId();
            var username = GetCurrentUsername();

            if (userId == null || username == null)
            {
                return RedirectToActionWithError("Login", "Auth", "Invalid session. Please login again.");
            }

            // Get recent posts from all users (Feed)
            var recentPostsResult = await _postService.GetRecentPostsAsync(10);
            var recentPosts = recentPostsResult.IsSuccess ? recentPostsResult.Data! : Enumerable.Empty<Models.Post>();

            // Get user's own posts
            var userPostsResult = await _postService.GetUserPostsAsync(userId.Value);
            var userPosts = userPostsResult.IsSuccess ? userPostsResult.Data! : Enumerable.Empty<Models.Post>();

            // Calculate stats
            var totalPosts = userPosts.Count();
            var totalLikes = userPosts.Sum(p => p.LikesCount);
            var totalComments = userPosts.Sum(p => p.CommentsCount);
            var totalViews = userPosts.Sum(p => p.ViewsCount);

            var model = new DashboardViewModel
            {
                Username = username,
                TotalPosts = totalPosts,
                TotalReach = totalViews,
                TotalSaved = 0, // TODO: Implement saved places
                TotalLikes = totalLikes,

                // Map recent posts to view models
                RecentPosts = recentPosts.Select(p => new PostCardViewModel
                {
                    Id = p.Id,
                    Username = p.User.Username,
                    UserInitials = GetInitials(p.User.Username),
                    Location = p.Location ?? "Unknown",
                    Content = p.Content,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    TimeAgo = GetTimeAgo(p.CreatedAt),
                    Category = DetermineCategory(p.Content, p.Location)
                }).ToList(),

                // Map user's posts
                YourPosts = userPosts.Select(p => new PostCardViewModel
                {
                    Id = p.Id,
                    Username = username,
                    UserInitials = GetInitials(username),
                    Location = p.Location ?? "Unknown",
                    Content = p.Content,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    TimeAgo = GetTimeAgo(p.CreatedAt),
                    Category = DetermineCategory(p.Content, p.Location)
                }).ToList(),

                // TODO: Get real saved places from database
                SavedPlaces = new List<SavedPlaceViewModel>()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An error occurred while loading the dashboard.", ex);
            return RedirectToAction("Index", "Home");
        }
    }

    private static string GetInitials(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "U";

        var parts = username.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();

        return username[0].ToString().ToUpper();
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) != 1 ? "s" : "")} ago";

        return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) != 1 ? "s" : "")} ago";
    }

    private static string DetermineCategory(string content, string? location)
    {
        var text = (content + " " + location).ToLower();

        if (text.Contains("beach") || text.Contains("island") || text.Contains("surf") || text.Contains("dive"))
            return "Adventure";
        if (text.Contains("food") || text.Contains("restaurant") || text.Contains("lechon") || text.Contains("eat"))
            return "Food";
        if (text.Contains("culture") || text.Contains("history") || text.Contains("church") || text.Contains("museum"))
            return "Culture";
        if (text.Contains("nature") || text.Contains("mountain") || text.Contains("hike") || text.Contains("waterfall"))
            return "Nature";

        return "Travel";
    }
}

