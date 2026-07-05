using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Commons.Behaviours;

public class UnhandledExceptionBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : IMessage
{
    private readonly ILogger<UnhandledExceptionBehaviour<TMessage, TResponse>> _logger;

    public UnhandledExceptionBehaviour(ILogger<UnhandledExceptionBehaviour<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TMessage).Name;
            var userName = "Unknown";

            _logger.LogError(ex, "Request: Unhandled Exception for Request {Name} by {@UserName} {@Request}", requestName, userName, message);

            throw;
        }
    }
}