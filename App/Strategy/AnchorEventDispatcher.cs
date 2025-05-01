using Domain.Interfaces.Hooks.Parsing;
using Domain.Interfaces.Strategy;

namespace App.Strategy;

public sealed class AnchorEventDispatcher(IServiceProvider provider, IEnumerable<IAnchorEventHandler> handlers)
{
    public async Task DispatchAsync(IAnchorEvent ev, CancellationToken ct = default)
    {
        var handler = handlers.FirstOrDefault(h => h.CanHandle(ev));

        if (handler is null)
            throw new InvalidOperationException(
                $"No handler registered for {ev.GetType().Name}");

        await handler.HandleAsync(ev, ct);
    }
}