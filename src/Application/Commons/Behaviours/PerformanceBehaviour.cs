using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Application.Commons.Behaviours;

public class PerformanceBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
     where TMessage : IMessage
{
    private readonly ILogger<PerformanceBehaviour<TMessage, TResponse>> _logger;
    private readonly Stopwatch _timer;

    public PerformanceBehaviour(ILogger<PerformanceBehaviour<TMessage, TResponse>> logger)
    {
        _logger = logger;
        _timer = new Stopwatch();
    }

    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next(message, cancellationToken);

        _timer.Stop();

        if (_timer.ElapsedMilliseconds > 500)
        {
            var requestName = typeof(TMessage).Name;
            var userName = "Unknown";

            _logger.LogWarning(
                "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) by {@UserName} {@Request}",
                requestName,
                _timer.ElapsedMilliseconds,
                userName,
                message);
        }

        return response;
    }
}