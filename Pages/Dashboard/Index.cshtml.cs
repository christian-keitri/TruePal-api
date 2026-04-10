using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Pages.Base;

namespace TruePal.Api.Pages.Dashboard;

public class IndexModel : BasePageModel
{
    public IndexModel(ILogger<IndexModel> logger) : base(logger)
    {
    }

    public string? Username { get; set; }

    public IActionResult OnGet()
    {
        // Check if user is logged in
        if (!Request.Cookies.ContainsKey("AuthToken"))
        {
            return RedirectToPageWithError("/Auth/Login", "Please login to access the dashboard");
        }

        // TODO: Get username from JWT token
        Username = "User";

        return Page();
    }
}
