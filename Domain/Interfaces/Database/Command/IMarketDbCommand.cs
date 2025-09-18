using Domain.Models.Dtos;
using Domain.Models.Events;

namespace Domain.Interfaces.Database.Command;

public interface IMarketDbCommand
{
    Task CreateSellerOfferAsync(OfferInitialized offer);
    Task CreateBuyerOfferAsync(BuyOrderInitialized offer);

    Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto upsertOrderDto);
    Task UpdateCurrentOfferAsync(UpsertOrderDto  upsertOrder);
}