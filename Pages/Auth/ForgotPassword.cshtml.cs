using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Pages.Base;

namespace TruePal.Api.Pages.Auth;

public class ForgotPasswordModel : BasePageModel
{
    public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger) : base(logger)
    {
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Implement password reset functionality
        SetInfo("Password reset functionality coming soon!");
        _logger.LogInformation("Password reset requested for {Email}", Email);
        
        return Page();
    }
}
