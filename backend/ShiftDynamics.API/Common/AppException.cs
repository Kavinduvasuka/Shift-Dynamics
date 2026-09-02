namespace ShiftDynamics.API.Common;

/// <summary>
/// Base application exception that carries an HTTP status code and optional error code.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }
    public string? Code { get; }

    public AppException(string message, int statusCode = 400, string? code = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message = "Resource not found.")
        : base(message, StatusCodes.Status404NotFound, "not_found")
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message = "Conflict occurred.")
        : base(message, StatusCodes.Status409Conflict, "conflict")
    {
    }
}

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, StatusCodes.Status403Forbidden, "forbidden")
    {
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base(message, StatusCodes.Status401Unauthorized, "unauthorized")
    {
    }
}

public class ValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]>? errors = null)
        : base(message, StatusCodes.Status400BadRequest, "validation_error")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}
