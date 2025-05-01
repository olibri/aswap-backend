using Domain.Interfaces.Hooks.Parsing;

namespace Domain.Interfaces.Strategy;

public interface IAnchorEventHandler
{
    bool CanHandle(IAnchorEvent ev);

    Task HandleAsync(IAnchorEvent ev, CancellationToken ct = default);
}