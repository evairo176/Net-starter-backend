namespace NetStarter.Api.Services;

/// <summary>
/// Hasil layanan di layer service, dipetakan ke HTTP di controller (pola project-management).
/// IsSuccess == true -> controller balikin ApiResult.Ok(...)
/// IsSuccess == false -> controller balikin StatusCode(result.StatusCode, ApiResult.Error(...))
/// </summary>
public record ServiceResult<T>(T? Data, string? Error, int StatusCode = 200)
{
    public bool IsSuccess => Error is null;
    public static ServiceResult<T> Success(T data) => new(data, null);
    public static ServiceResult<T> NotFound(string error) => new(default, error, 404);
    public static ServiceResult<T> Conflict(string error) => new(default, error, 409);
    public static ServiceResult<T> BadRequest(string error) => new(default, error, 400);
    public static ServiceResult<T> Forbidden(string error) => new(default, error, 403);
    public static ServiceResult<T> Fail(string error) => new(default, error, 500);
}