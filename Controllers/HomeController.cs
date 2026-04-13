using System.ComponentModel.DataAnnotations;
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
        var model = new HomeViewModel
        {
            IsAuthenticated = Request.Cookies.ContainsKey("AuthToken")
        };
        return View(model);
    }

    public IActionResult Error()
    {
        return View();
    }
}

#region ViewModels

public class HomeViewModel
{
    public bool IsAuthenticated { get; set; }
}

#endregion
