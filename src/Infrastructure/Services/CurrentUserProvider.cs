using Domain.Common;

namespace Infrastructure.Services;

// Default: returns "System". Replace with auth-backed implementation in WebUi
// (e.g. resolve from HttpContext.User or Blazor AuthenticationStateProvider).
public sealed class CurrentUserProvider : ICurrentUserProvider
{
    public string UserName => "System";
}
