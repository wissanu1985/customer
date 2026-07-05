using System.Net;

namespace Application.Commons.Exceptions;

public abstract class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public IReadOnlyList<string> Messages { get; }

    protected AppException(HttpStatusCode statusCode, IReadOnlyList<string> messages)
        : base(messages.FirstOrDefault() ?? "An error occurred")
    {
        StatusCode = statusCode;
        Messages = messages;
    }

    protected AppException(HttpStatusCode statusCode, string message)
        : this(statusCode, new[] { message }) { }
}

public sealed class ValidationAppException : AppException
{
    public ValidationAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.UnprocessableEntity, messages) { }

    public ValidationAppException(string message)
        : base(HttpStatusCode.UnprocessableEntity, message) { }
}

public sealed class BadRequestAppException : AppException
{
    public BadRequestAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.BadRequest, messages) { }

    public BadRequestAppException(string message)
        : base(HttpStatusCode.BadRequest, message) { }
}

public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.NotFound, messages) { }

    public NotFoundAppException(string message)
        : base(HttpStatusCode.NotFound, message) { }
}

public sealed class ConflictAppException : AppException
{
    public ConflictAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.Conflict, messages) { }

    public ConflictAppException(string message)
        : base(HttpStatusCode.Conflict, message) { }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.Unauthorized, messages) { }

    public UnauthorizedAppException(string message)
        : base(HttpStatusCode.Unauthorized, message) { }
}

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.Forbidden, messages) { }

    public ForbiddenAppException(string message)
        : base(HttpStatusCode.Forbidden, message) { }
}

public sealed class SystemAppException : AppException
{
    public SystemAppException(IReadOnlyList<string> messages)
        : base(HttpStatusCode.InternalServerError, messages) { }

    public SystemAppException(string message)
        : base(HttpStatusCode.InternalServerError, message) { }
}