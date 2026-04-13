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

        // TODO: Get user from JWT token
        ViewData["Username"] = "User";
        ViewData["Email"] = "user@example.com";
        ViewData["CreatedAt"] = DateTime.UtcNow;

        return View();
    }
}
