namespace NetStarter.Api.Dtos.Api;

/// <summary>
/// Global helper untuk membuat API response secara konsisten (copy pola project-management).
///
/// Usage examples:
///   ApiResult.Ok(data)                           -> { success: true, message: "Success", data: ..., meta: null }
///   ApiResult.Ok(data, "Data berhasil disimpan") -> { success: true, message: "...", data: ..., meta: null }
///   ApiResult.Paged(items, page, limit, total)   -> { success: true, ..., data: [...], meta: { page, limit, ... } }
///   ApiResult.NotFound("Product tidak ada")      -> { success: false, message: "...", data: null, code: "NOT_FOUND" }
/// </summary>
public static class ApiResult
{
    /// <summary>Single item success response (meta = null).</summary>
    public static SuccessApiResponse<T> Ok<T>(T data, string message = "Success")
        => new(true, message, data, null);

    /// <summary>Created (201) response.</summary>
    public static SuccessApiResponse<T> Created<T>(T data, string message = "Berhasil dibuat")
        => new(true, message, data, null);

    /// <summary>Paginated list response with meta.</summary>
    public static SuccessApiResponse<IEnumerable<T>> Paged<T>(
        IEnumerable<T> items,
        int page,
        int limit,
        long total)
    {
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);
        var meta = new PaginationMeta
        {
            Page = page,
            Limit = limit,
            TotalItems = (int)total,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1,
        };
        return new SuccessApiResponse<IEnumerable<T>>(true, "Success", items, meta);
    }

    /// <summary>Generic error response.</summary>
    public static ErrorApiResponse Error(string message, string? code = null)
        => new(false, message, code);

    /// <summary>Not found (404) error.</summary>
    public static ErrorApiResponse NotFound(string message = "Data tidak ditemukan")
        => new(false, message, "NOT_FOUND");

    /// <summary>Validation error (400).</summary>
    public static ErrorApiResponse BadRequest(string message)
        => new(false, message, "VALIDATION_ERROR");

    /// <summary>Forbidden (403) error.</summary>
    public static ErrorApiResponse Forbidden(string message = "Akses ditolak")
        => new(false, message, "FORBIDDEN");

    /// <summary>Conflict (409) error.</summary>
    public static ErrorApiResponse Conflict(string message)
        => new(false, message, "CONFLICT");

    /// <summary>Internal (500) error.</summary>
    public static ErrorApiResponse Internal(string message = "Terjadi kesalahan")
        => new(false, message, "INTERNAL_ERROR");
}

/// <summary>Successful response wrapper - data + optional pagination meta.</summary>
public class SuccessApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public T Data { get; init; }
    public PaginationMeta? Meta { get; init; }

    public SuccessApiResponse(bool success, string message, T data, PaginationMeta? meta)
    {
        Success = success;
        Message = message;
        Data = data;
        Meta = meta;
    }
}

/// <summary>Error response wrapper - no data, just message + code.</summary>
public class ErrorApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public string? Code { get; init; }

    public ErrorApiResponse(bool success, string message, string? code = null)
    {
        Success = success;
        Message = message;
        Code = code;
    }
}

/// <summary>Pagination metadata included in paginated responses.</summary>
public record PaginationMeta
{
    public int Page { get; init; }
    public int Limit { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}