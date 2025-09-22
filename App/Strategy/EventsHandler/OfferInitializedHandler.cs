using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Hooks.Parsing;
using Domain.Interfaces.Strategy;
using Domain.Models.Events;

namespace App.Strategy.EventsHandler;

public sealed class OfferInitializedHandler(IMarketDbCommand marketDbCommand)
    : IAnchorEventHandler
{
    public bool CanHandle(IAnchorEvent ev) => ev is UniversalOrderCreated;
    public async Task HandleAsync(IAnchorEvent ev, CancellationToken ct = default)
    {
        await marketDbCommand.CreateOfferAsync((UniversalOrderCreated)ev);
    }
}