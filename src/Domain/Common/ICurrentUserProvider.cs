namespace Domain.Common;

// Abstraction for the current actor performing a change.
// Default implementation returns "System"; override in WebUi with real auth user.
public interface ICurrentUserProvider
{
    string UserName { get; }
}
