using System.Text.Json.Serialization;

namespace StayHub.Shared.Web.Response;

/// <summary>
/// Standard API response envelope for all StayHub endpoints.
/// Ensures consistent response structure across all microservices.
/// </summary>
#pragma warning disable CA1000
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<ApiError>? Errors { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMeta? Meta { get; init; }

    public static ApiResponse<T> Ok(T data, PaginationMeta? meta = null) => new()
    {
        Success = true,
        Data = data,
        Meta = meta
    };

    public static ApiResponse<T> Fail(IEnumerable<ApiError> errors) => new()
    {
        Success = false,
        Errors = errors
    };

    public static ApiResponse<T> Fail(string code, string message) => new()
    {
        Success = false,
        Errors = [new ApiError(code, message)]
    };
}

/// <summary>
/// Non-generic version for endpoints that return no data (204, etc.).
/// </summary>
public sealed class ApiResponse
{
    public bool Success { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<ApiError>? Errors { get; init; }

    public static ApiResponse Ok() => new() { Success = true };

    public static ApiResponse Fail(IEnumerable<ApiError> errors) => new()
    {
        Success = false,
        Errors = errors
    };

    public static ApiResponse Fail(string code, string message) => new()
    {
        Success = false,
        Errors = [new ApiError(code, message)]
    };
}

public sealed record ApiError(string Code, string Message);

public sealed record PaginationMeta(int Page, int PageSize, int TotalCount, int TotalPages);
