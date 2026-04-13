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

        // TODO: Get username from JWT token
        ViewData["Username"] = "User";

        return View();
    }
}
