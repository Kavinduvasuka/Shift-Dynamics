using System.Net;
using System.Text.Json;
using ShiftDynamics.API.Common;

namespace ShiftDynamics.API.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns a consistent ApiError JSON payload.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        int statusCode;
        string message;
        string? code = null;
        IDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case ValidationException ve:
                statusCode = ve.StatusCode;
                message = ve.Message;
                code = ve.Code;
                errors = ve.Errors;
                _logger.LogWarning(exception, "Validation error. TraceId={TraceId}", traceId);
                break;

            case AppException appEx:
                statusCode = appEx.StatusCode;
                message = appEx.Message;
                code = appEx.Code;
                _logger.LogWarning(exception, "Application error ({Code}). TraceId={TraceId}", code, traceId);
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred.";
                code = "internal_error";
                _logger.LogError(exception, "Unhandled exception. TraceId={TraceId}", traceId);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var payload = new ApiError
        {
            Success = false,
            Message = message,
            Code = code,
            Errors = errors,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
