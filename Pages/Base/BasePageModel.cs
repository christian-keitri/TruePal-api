using System.Text.Json;
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

    [TempData]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Stored as JSON string so it survives TempData cookie serialization.
    /// </summary>
    [TempData]
    public string? ErrorMessagesJson { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? InfoMessage { get; set; }

    [TempData]
    public string? WarningMessage { get; set; }

    public List<string> ErrorMessages
    {
        get => string.IsNullOrEmpty(ErrorMessagesJson)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(ErrorMessagesJson) ?? new List<string>();
    }

    public bool HasErrors => !string.IsNullOrEmpty(ErrorMessage) || !string.IsNullOrEmpty(ErrorMessagesJson);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
    public bool HasInfo => !string.IsNullOrEmpty(InfoMessage);

    protected void AddError(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            var list = ErrorMessages;
            list.Add(message);
            ErrorMessagesJson = JsonSerializer.Serialize(list);
        }
    }

    protected void AddErrors(IEnumerable<string> messages)
    {
        if (messages != null && messages.Any())
        {
            var list = ErrorMessages;
            list.AddRange(messages);
            ErrorMessagesJson = JsonSerializer.Serialize(list);
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
