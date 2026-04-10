using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Pages.Base;
using TruePal.Api.Pages.ViewModels;

namespace TruePal.Api.Pages.Auth;

public class LoginModel : BasePageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService, ILogger<LoginModel> logger) : base(logger)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _authService.LoginAsync(Input.Email, Input.Password);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                {
                    AddErrors(result.Errors);
                }
                else
                {
                    ErrorMessage = result.Error;
                }
                
                _logger.LogWarning("Failed login attempt for {Email}", Input.Email);
                return Page();
            }

            // Store the token in a cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = Input.RememberMe 
                    ? DateTimeOffset.UtcNow.AddDays(30) 
                    : DateTimeOffset.UtcNow.AddMinutes(60)
            };

            Response.Cookies.Append("AuthToken", result.Data!, cookieOptions);

            _logger.LogInformation("User {Email} logged in successfully", Input.Email);

            return LocalRedirect(ReturnUrl);
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An unexpected error occurred during login. Please try again.", ex);
            return Page();
        }
    }

    public IActionResult OnPostLogout()
    {
        Response.Cookies.Delete("AuthToken");
        _logger.LogInformation("User logged out");
        return RedirectToPageWithSuccess("/Index", "You have been logged out successfully");
    }
}
