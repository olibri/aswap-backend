using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Hooks.Parsing;
using Domain.Interfaces.Strategy;
using Domain.Models.Events;

namespace App.Strategy.EventsHandler;

public sealed class EscrowInitializedHandler(IMarketDbCommand marketDbCommand)
    : IAnchorEventHandler
{
    public bool CanHandle(IAnchorEvent ev) => ev is EscrowInitialized;
    public Task HandleAsync(IAnchorEvent ev, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}