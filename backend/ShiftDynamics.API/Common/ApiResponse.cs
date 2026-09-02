namespace ShiftDynamics.API.Common;

/// <summary>
/// Standard success envelope for API responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public string? Message { get; init; }
    public T? Data { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message, Data = default };
}

/// <summary>
/// Non-generic success envelope.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; } = true;
    public string? Message { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ApiResponse Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>
/// Standard error payload returned for 4xx/5xx responses.
/// </summary>
public class ApiError
{
    public bool Success { get; init; } = false;
    public string Message { get; init; } = string.Empty;
    public string? Code { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }
    public string? TraceId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
