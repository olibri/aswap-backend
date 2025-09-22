using Domain.Models.Dtos;
using Domain.Models.Events;

namespace Domain.Interfaces.Database.Command;

public interface IMarketDbCommand
{
    Task CreateOfferAsync(UniversalOrderCreated offer);

    Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto upsertOrderDto);
    Task UpdateCurrentOfferAsync(UpsertOrderDto  upsertOrder);
}