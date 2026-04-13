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
}
