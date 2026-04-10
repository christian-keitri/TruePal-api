using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TruePal.Api.Pages.Base;

public abstract class BasePageModel : PageModel
{
    protected readonly ILogger _logger;

    protected BasePageModel(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Display a single error message
    /// </summary>
    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Display multiple error messages
    /// </summary>
    [TempData]
    public List<string>? ErrorMessages { get; set; }

    /// <summary>
    /// Display a success message
    /// </summary>
    [TempData]
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Display an info message
    /// </summary>
    [TempData]
    public string? InfoMessage { get; set; }

    /// <summary>
    /// Check if there are any error messages
    /// </summary>
    public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage) || (ErrorMessages?.Any() ?? false);

    /// <summary>
    /// Check if there is a success message
    /// </summary>
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    /// <summary>
    /// Check if there is an info message
    /// </summary>
    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);

    /// <summary>
    /// Add a single error message
    /// </summary>
    protected void AddError(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            ErrorMessages ??= new List<string>();
            ErrorMessages.Add(message);
        }
    }

    /// <summary>
    /// Add multiple error messages
    /// </summary>
    protected void AddErrors(IEnumerable<string> messages)
    {
        if (messages != null && messages.Any())
        {
            ErrorMessages ??= new List<string>();
            ErrorMessages.AddRange(messages);
        }
    }

    /// <summary>
    /// Set a success message
    /// </summary>
    protected void SetSuccess(string message)
    {
        SuccessMessage = message;
    }

    /// <summary>
    /// Set an info message
    /// </summary>
    protected void SetInfo(string message)
    {
        InfoMessage = message;
    }

    /// <summary>
    /// Redirect to another page with a success message
    /// </summary>
    protected IActionResult RedirectToPageWithSuccess(string pageName, string message)
    {
        SetSuccess(message);
        return RedirectToPage(pageName);
    }

    /// <summary>
    /// Redirect to another page with an error message
    /// </summary>
    protected IActionResult RedirectToPageWithError(string pageName, string message)
    {
        ErrorMessage = message;
        return RedirectToPage(pageName);
    }

    /// <summary>
    /// Log and display an error
    /// </summary>
    protected void LogAndDisplayError(string message, Exception? exception = null)
    {
        if (exception != null)
        {
            _logger.LogError(exception, message);
        }
        else
        {
            _logger.LogError(message);
        }
        ErrorMessage = message;
    }
}
