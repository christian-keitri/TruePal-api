using System.ComponentModel.DataAnnotations;

namespace TruePal.Api.Core.Validators;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static List<string> ValidateRegister(string username, string email, string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(username))
            errors.Add("Username is required");
        else if (username.Length < 3)
            errors.Add("Username must be at least 3 characters");
        else if (username.Length > 50)
            errors.Add("Username must not exceed 50 characters");

        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email is required");
        else if (!IsValidEmail(email))
            errors.Add("Email is not valid");

        if (string.IsNullOrWhiteSpace(password))
            errors.Add("Password is required");
        else if (password.Length < 6)
            errors.Add("Password must be at least 6 characters");

        return errors;
    }

    public static List<string> ValidateLogin(string email, string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email is required");
        else if (!IsValidEmail(email))
            errors.Add("Email is not valid");

        if (string.IsNullOrWhiteSpace(password))
            errors.Add("Password is required");

        return errors;
    }
}
