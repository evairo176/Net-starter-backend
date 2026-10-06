// ==== API Response Standard - satu envelope utk semua endpoint ====
// shape: { success, message, data, error }

namespace NetStarter.Api.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, string code, object? details = null) =>
        new() { Success = false, Message = message, Error = new ApiError { Code = code, Details = details } };
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public object? Details { get; set; }
}

public static class ErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string BadRequest = "BAD_REQUEST";
    public const string Internal = "INTERNAL_ERROR";
}