using System.Diagnostics.CodeAnalysis;

namespace Application.Commons.Exceptions;

/// <summary>
/// Fluent helper for throwing typed application exceptions.
/// Usage: ExceptionHelper.Validation(["Id must not be empty"]).Throw();
/// </summary>
public static class ExceptionHelper
{
    public static ExceptionContext Validation(IEnumerable<string> messages)
        => new(new ValidationAppException(messages.ToList()));

    public static ExceptionContext Validation(string message)
        => new(new ValidationAppException(message));

    public static ExceptionContext BadRequest(IEnumerable<string> messages)
        => new(new BadRequestAppException(messages.ToList()));

    public static ExceptionContext BadRequest(string message)
        => new(new BadRequestAppException(message));

    public static ExceptionContext NotFound(IEnumerable<string> messages)
        => new(new NotFoundAppException(messages.ToList()));

    public static ExceptionContext NotFound(string message)
        => new(new NotFoundAppException(message));

    public static ExceptionContext Conflict(IEnumerable<string> messages)
        => new(new ConflictAppException(messages.ToList()));

    public static ExceptionContext Conflict(string message)
        => new(new ConflictAppException(message));

    public static ExceptionContext Unauthorized(IEnumerable<string> messages)
        => new(new UnauthorizedAppException(messages.ToList()));

    public static ExceptionContext Unauthorized(string message)
        => new(new UnauthorizedAppException(message));

    public static ExceptionContext Forbidden(IEnumerable<string> messages)
        => new(new ForbiddenAppException(messages.ToList()));

    public static ExceptionContext Forbidden(string message)
        => new(new ForbiddenAppException(message));

    public static ExceptionContext System(IEnumerable<string> messages)
        => new(new SystemAppException(messages.ToList()));

    public static ExceptionContext System(string message)
        => new(new SystemAppException(message));
}

public sealed class ExceptionContext
{
    private readonly AppException _exception;

    internal ExceptionContext(AppException exception)
    {
        _exception = exception;
    }

    /// <summary>Throws the wrapped exception. This method never returns.</summary>
    [DoesNotReturn]
    public void Throw() => throw _exception;

    /// <summary>Returns the wrapped exception without throwing (useful for re-throwing or logging).</summary>
    public AppException Exception => _exception;
}