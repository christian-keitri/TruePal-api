using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TruePal.Api.Pages;

public class IndexModel : PageModel
{
    public bool IsAuthenticated { get; set; }

    public void OnGet()
    {
        IsAuthenticated = Request.Cookies.ContainsKey("AuthToken");
    }
}
