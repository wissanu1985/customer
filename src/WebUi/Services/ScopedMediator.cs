using Mediator;

namespace WebUi.Services;

/// <summary>
/// Wraps IMediator so each Send() runs in a fresh DI scope.
/// In Blazor Server, scoped services live for the whole circuit — meaning a single DbContext
/// is shared across concurrent operations, causing "second operation started" errors.
/// By creating a new scope per Send(), each call gets its own handler, UnitOfWork, and DbContext,
/// which are disposed when the scope ends.
/// </summary>
public sealed class ScopedMediator
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScopedMediator(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(request, cancellationToken);
    }
}
