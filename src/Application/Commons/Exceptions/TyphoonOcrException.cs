namespace Application.Commons.Exceptions;

public sealed class TyphoonOcrException : Exception
{
    public int StatusCode { get; }

    public TyphoonOcrException(int statusCode, string message, string? responseBody = null)
        : base(message + (responseBody is null ? "" : $" | Body: {responseBody}"))
    {
        StatusCode = statusCode;
    }
}
