using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Controllers.Base;
using TruePal.Api.Core.Interfaces;

namespace TruePal.Api.Controllers;

public class ProfileController : BaseController
{
    private readonly IUnitOfWork _unitOfWork;

    public ProfileController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ProfileController> logger) 
        : base(logger, configuration)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToActionWithError("Login", "Auth", "Please login to access your profile");
        }

        // TODO: Get user from JWT token and load actual data
        var model = new ProfileViewModel
        {
            Username = "User",
            Email = "user@example.com",
            CreatedAt = DateTime.UtcNow
        };

        return View(model);
    }
}

#region ViewModels

public class ProfileViewModel
{
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Member Since")]
    public DateTime CreatedAt { get; set; }
}

#endregion
