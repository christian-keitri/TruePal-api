using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruePal.Api.Core.Interfaces;
using TruePal.Api.Controllers.Base;

namespace TruePal.Api.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger) 
        : base(logger, configuration)
    {
        _authService = authService;
    }

    #region Login

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authService.LoginAsync(model.Email, model.Password);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                {
                    AddErrors(result.Errors);
                }
                else
                {
                    SetError(result.Error!);
                }
                
                _logger.LogWarning("Failed login attempt for {Email}", model.Email);
                return View(model);
            }

            // Store the token in a cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = model.RememberMe 
                    ? DateTimeOffset.UtcNow.AddDays(30) 
                    : DateTimeOffset.UtcNow.AddMinutes(60)
            };

            Response.Cookies.Append("AuthToken", result.Data!, cookieOptions);

            _logger.LogInformation("User {Email} logged in successfully", model.Email);

            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An unexpected error occurred during login. Please try again.", ex);
            return View(model);
        }
    }

    #endregion

    #region Register

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authService.RegisterAsync(model.Username, model.Email, model.Password);

            if (!result.IsSuccess)
            {
                if (result.Errors.Any())
                {
                    AddErrors(result.Errors);
                }
                else
                {
                    SetError(result.Error!);
                }

                _logger.LogWarning("Failed registration attempt for {Email}", model.Email);
                return View(model);
            }

            _logger.LogInformation("New user {Email} registered successfully", model.Email);

            return RedirectToActionWithSuccess("Login", "Auth", 
                "Registration successful! Please login with your credentials.");
        }
        catch (Exception ex)
        {
            LogAndDisplayError("An unexpected error occurred during registration. Please try again.", ex);
            return View(model);
        }
    }

    #endregion

    #region Forgot Password

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // TODO: Implement password reset functionality
        SetInfo("Password reset functionality coming soon!");
        _logger.LogInformation("Password reset requested for {Email}", model.Email);
        
        return View(model);
    }

    #endregion

    #region Logout

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("AuthToken");
        _logger.LogInformation("User logged out");
        return RedirectToActionWithSuccess("Index", "Home", "You have been logged out successfully");
    }

    #endregion
}

#region ViewModels

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}

#endregion