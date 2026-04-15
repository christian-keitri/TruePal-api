namespace TruePal.Api.Core.Interfaces;

public static class ErrorCodes
{
    public const string NotFound = "NOT_FOUND";
    public const string Forbidden = "FORBIDDEN";
    public const string Validation = "VALIDATION";
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public List<string> Errors { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, T? data, string? error, List<string>? errors = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors ?? new List<string>();
        ErrorCode = errorCode;
        
        // Add single error to Errors list if provided and Errors list is empty
        if (!string.IsNullOrEmpty(error) && Errors.Count == 0)
        {
            Errors.Add(error);
        }
    }

    public static Result<T> Success(T data) => new(true, data, null);

    public static Result<T> Failure(string error, string? errorCode = null) => new(false, default, error, errorCode: errorCode);

    public static Result<T> Failure(List<string> errors) => new(false, default, null, errors, ErrorCodes.Validation);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string> Errors { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, List<string>? errors = null, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? new List<string>();
        ErrorCode = errorCode;
        
        // Add single error to Errors list if provided and Errors list is empty
        if (!string.IsNullOrEmpty(error) && Errors.Count == 0)
        {
            Errors.Add(error);
        }
    }

    public static Result Success() => new(true, null);

    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode: errorCode);

    public static Result Failure(List<string> errors) => new(false, null, errors, ErrorCodes.Validation);
}
