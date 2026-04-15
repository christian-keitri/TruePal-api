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
    private readonly IUserService _userService;
    private readonly IFileService _fileService;

    public ProfileController(
        IUnitOfWork unitOfWork, 
        IPostService postService,
        IUserService userService,
        IFileService fileService,
        IConfiguration configuration, 
        ILogger<ProfileController> logger) 
        : base(logger, configuration)
    {
        _unitOfWork = unitOfWork;
        _postService = postService;
        _userService = userService;
        _fileService = fileService;
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
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
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

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToActionWithError("Login", "Auth", "Please login to edit your profile");
        }

        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToActionWithError("Login", "Auth", "Invalid session. Please login again.");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToActionWithError("Login", "Auth", "User not found. Please login again.");
            }

            var model = new EditProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                CurrentProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An error occurred while loading the edit profile page.", ex);
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToActionWithError("Login", "Auth", "Please login to edit your profile");
        }

        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToActionWithError("Login", "Auth", "Invalid session. Please login again.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? profilePictureUrl = model.CurrentProfilePictureUrl;

            // Handle profile picture upload
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                using (var stream = model.ProfilePicture.OpenReadStream())
                {
                    var uploadResult = await _fileService.UploadFileAsync(
                        stream, 
                        model.ProfilePicture.FileName, 
                        model.ProfilePicture.ContentType
                    );

                    if (!uploadResult.IsSuccess)
                    {
                        AddErrors(uploadResult.Errors);
                        return View(model);
                    }

                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(model.CurrentProfilePictureUrl))
                    {
                        await _fileService.DeleteFileAsync(model.CurrentProfilePictureUrl);
                    }

                    profilePictureUrl = uploadResult.Data;
                }
            }

            // Update profile
            var result = await _userService.UpdateProfileAsync(userId.Value, model.Bio, profilePictureUrl);

            if (!result.IsSuccess)
            {
                AddErrors(result.Errors);
                return View(model);
            }

            return RedirectToActionWithSuccess("Index", "Profile", "Profile updated successfully!");
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An error occurred while updating your profile.", ex);
            return View(model);
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
