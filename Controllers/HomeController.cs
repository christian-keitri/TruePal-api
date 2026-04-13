using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Controllers.Base;

namespace TruePal.Api.Controllers;

public class HomeController : BaseController
{
    public HomeController(ILogger<HomeController> logger) : base(logger)
    {
    }

    public IActionResult Index()
    {
        ViewData["IsAuthenticated"] = Request.Cookies.ContainsKey("AuthToken");
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
