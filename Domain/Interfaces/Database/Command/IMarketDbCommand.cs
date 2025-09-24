using Domain.Models.Dtos;
using Domain.Models.Events;

namespace Domain.Interfaces.Database.Command;

public interface IMarketDbCommand
{
  Task CreateOfferAsync(UniversalOrderCreated offer);
  Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto upsertOrderDto);

  // Was: Task UpdateCurrentOfferAsync(UpsertOrderDto upsertOrder)
  Task<EscrowOrderDto> UpdateCurrentOfferAsync(UpsertOrderDto upsertOrder, CancellationToken ct = default);
}