using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Pages.Base;
using TruePal.Api.Pages.ViewModels;

namespace TruePal.Api.Pages.Auth;

public class RegisterModel : BasePageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger) : base(logger)
    {
        _authService = authService;
    }

    [BindProperty]
    public RegisterViewModel Input { get; set; } = new();

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
            var result = await _authService.RegisterAsync(Input.Username, Input.Email, Input.Password);

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

                _logger.LogWarning("Failed registration attempt for {Email}", Input.Email);
                return Page();
            }

            _logger.LogInformation("New user {Email} registered successfully", Input.Email);

            // Registration successful, redirect to login with success message
            return RedirectToPageWithSuccess("/Auth/Login", 
                "Registration successful! Please login with your credentials.");
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An unexpected error occurred during registration. Please try again.", ex);
            return Page();
        }
    }
}
