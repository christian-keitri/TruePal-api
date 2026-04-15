using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TruePal.Api.Controllers.Base;

public abstract class BaseController : Controller
{
    protected readonly ILogger _logger;
    protected readonly IConfiguration _configuration;

    protected BaseController(ILogger logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // Parameterless constructor for backward compatibility
    protected BaseController(ILogger logger)
    {
        _logger = logger;
        _configuration = null!;
    }

    #region Success/Error Messages

    protected void SetSuccess(string message)
    {
        TempData["SuccessMessage"] = message;
    }

    protected void SetError(string message)
    {
        TempData["ErrorMessage"] = message;
    }

    protected void SetInfo(string message)
    {
        TempData["InfoMessage"] = message;
    }

    protected void SetWarning(string message)
    {
        TempData["WarningMessage"] = message;
    }

    #endregion

    #region Redirect Helpers

    protected IActionResult RedirectToActionWithSuccess(string action, string controller, string message)
    {
        SetSuccess(message);
        return RedirectToAction(action, controller);
    }

    protected IActionResult RedirectToActionWithError(string action, string controller, string message)
    {
        SetError(message);
        return RedirectToAction(action, controller);
    }

    protected IActionResult RedirectToActionWithInfo(string action, string controller, string message)
    {
        SetInfo(message);
        return RedirectToAction(action, controller);
    }

    protected IActionResult RedirectToActionWithWarning(string action, string controller, string message)
    {
        SetWarning(message);
        return RedirectToAction(action, controller);
    }

    #endregion

    #region ModelState Helpers

    protected void AddErrors(IEnumerable<string> errors)
    {
        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    protected void AddError(string key, string errorMessage)
    {
        ModelState.AddModelError(key, errorMessage);
    }

    #endregion

    #region Logging Helpers

    protected void LogAndDisplayError(string message, Exception? ex = null)
    {
        if (ex != null)
        {
            _logger.LogError(ex, message);
        }
        else
        {
            _logger.LogError(message);
        }
        SetError(message);
    }

    #endregion

    #region JWT Token Helpers

    /// <summary>
    /// Get the current user ID from the JWT token stored in cookies
    /// </summary>
    protected int? GetCurrentUserId()
    {
        try
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse user ID from JWT token");
            return null;
        }
    }

    /// <summary>
    /// Get the current username from the JWT token
    /// </summary>
    protected string? GetCurrentUsername()
    {
        try
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            return usernameClaim?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse username from JWT token");
            return null;
        }
    }

    /// <summary>
    /// Get the current user email from the JWT token
    /// </summary>
    protected string? GetCurrentUserEmail()
    {
        try
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            return emailClaim?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse email from JWT token");
            return null;
        }
    }

    #endregion
}
