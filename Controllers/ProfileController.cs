using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Controllers.Base;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.ViewModels;

namespace TruePal.Api.Controllers;

public class ProfileController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostService _postService;

    public ProfileController(
        IUnitOfWork unitOfWork, 
        IPostService postService,
        IConfiguration configuration, 
        ILogger<ProfileController> logger) 
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
            return RedirectToActionWithError("Login", "Auth", "Please login to access your profile");
        }

        try
        {
            // Get user ID from JWT token
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToActionWithError("Login", "Auth", "Invalid session. Please login again.");
            }

            // Get user data from database
            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToActionWithError("Login", "Auth", "User not found. Please login again.");
            }

            // Get user's posts
            var postsResult = await _postService.GetUserPostsAsync(userId.Value);
            var posts = postsResult.IsSuccess ? postsResult.Data! : Enumerable.Empty<Models.Post>();

            var model = new ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                TotalPosts = posts.Count(),
                YourPosts = posts.Select(p => new PostCardViewModel
                {
                    Id = p.Id,
                    Username = user.Username,
                    UserInitials = GetInitials(user.Username),
                    Location = p.Location ?? "Unknown",
                    Content = p.Content,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    TimeAgo = GetTimeAgo(p.CreatedAt),
                    Category = string.Empty
                }).ToList()
            };

            return View(model);
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An error occurred while loading your profile.", ex);
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
}
