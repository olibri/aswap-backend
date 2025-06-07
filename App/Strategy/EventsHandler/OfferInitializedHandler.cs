using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Hooks.Parsing;
using Domain.Interfaces.Strategy;
using Domain.Models.Events;

namespace App.Strategy.EventsHandler;

public sealed class OfferInitializedHandler(IMarketDbCommand marketDbCommand)
    : IAnchorEventHandler
{
    public bool CanHandle(IAnchorEvent ev) => ev is OfferInitialized;
    public async Task HandleAsync(IAnchorEvent ev, CancellationToken ct = default)
    {
        if (ev is not OfferInitialized offerInitialized)
            throw new ArgumentException("Invalid event type", nameof(ev));
        try
        {
            await marketDbCommand.CreateSellerOfferAsync(offerInitialized);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}