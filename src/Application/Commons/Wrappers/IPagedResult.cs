using System.Net;

namespace Application.Commons.Wrappers;

public interface IPagedResult<T>
{
    IReadOnlyList<T> Data { get; }
    int Page { get; }
    int Size { get; }
    int Total { get; }
    int TotalPages { get; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }
    bool IsSuccess { get; }
    IReadOnlyList<string> Messages { get; }
    HttpStatusCode? StatusCode { get; }
}

public class PagedResult<T> : IPagedResult<T>
{
    public IReadOnlyList<T> Data { get; }
    public int Page { get; }
    public int Size { get; }
    public int Total { get; }
    public int TotalPages { get; }
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }
    public bool IsSuccess { get; private set; }
    public IReadOnlyList<string> Messages { get; }
    public HttpStatusCode? StatusCode { get; }

    public PagedResult(IReadOnlyList<T> data, int page, int size, int total, IReadOnlyList<string>? messages = null, HttpStatusCode? statusCode = HttpStatusCode.OK)
    {
        Data = data;
        Page = page;
        Size = size;
        Total = total;
        TotalPages = (int)Math.Ceiling((double)total / size);
        HasNextPage = page < TotalPages;
        HasPreviousPage = page > 1;
        IsSuccess = true;
        Messages = messages ?? Array.Empty<string>();
        StatusCode = statusCode;
    }

    public static PagedResult<T> Success(IReadOnlyList<T> data, int page, int size, int total, IReadOnlyList<string>? messages = null, HttpStatusCode? statusCode = HttpStatusCode.OK)
    {
        return new PagedResult<T>(data, page, size, total, messages, statusCode);
    }

    public static PagedResult<T> Failure(IReadOnlyList<string> messages, HttpStatusCode? statusCode = null)
    {
        return new PagedResult<T>(Array.Empty<T>(), 1, 1, 0, messages, statusCode)
        {
            IsSuccess = false
        };
    }

    public static PagedResult<T> Failure(string message, HttpStatusCode? statusCode = null)
    {
        return new PagedResult<T>(Array.Empty<T>(), 1, 1, 0, new[] { message }, statusCode)
        {
            IsSuccess = false
        };
    }
}