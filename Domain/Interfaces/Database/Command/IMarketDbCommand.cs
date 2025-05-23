using Domain.Models.Dtos;
using Domain.Models.Events;

namespace Domain.Interfaces.Database.Command;

public interface IMarketDbCommand
{
    Task CreateNewOfferAsync(OfferInitialized offer);
    Task UpdateCurrentOfferAsync(UpdateOrderDto  updateOrder);
}