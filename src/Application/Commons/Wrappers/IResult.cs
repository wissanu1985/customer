using System.Net;

namespace Application.Commons.Wrappers;

public interface IResult<T>
{
    T? Data { get; }
    bool IsSuccess { get; }
    IReadOnlyList<string> Messages { get; }
    HttpStatusCode? StatusCode { get; }
}

public class Result<T> : IResult<T>
{
    public T? Data { get; }
    public bool IsSuccess { get; }
    public IReadOnlyList<string> Messages { get; }
    public HttpStatusCode? StatusCode { get; }

    private Result(T? data, bool isSuccess, IReadOnlyList<string> messages, HttpStatusCode? statusCode = null)
    {
        Data = data;
        IsSuccess = isSuccess;
        Messages = messages;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T data, IReadOnlyList<string>? messages = null, HttpStatusCode? statusCode = HttpStatusCode.OK)
    {
        return new Result<T>(data, true, messages ?? Array.Empty<string>(), statusCode);
    }

    public static Result<T> Failure(IReadOnlyList<string> messages, HttpStatusCode? statusCode = null)
    {
        return new Result<T>(default, false, messages, statusCode);
    }

    public static Result<T> Failure(string message, HttpStatusCode? statusCode = null)
    {
        return new Result<T>(default, false, new[] { message }, statusCode);
    }
}