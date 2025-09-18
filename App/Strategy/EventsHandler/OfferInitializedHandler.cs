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
        switch (ev)
        {
            case OfferInitialized offerInit:
                if (offerInit.OfferType == 0) 
                    await marketDbCommand.CreateSellerOfferAsync(offerInit);
                else
                    await marketDbCommand.CreateBuyerOfferAsync(offerInit);
                break;
        }
    }
}