using Domain.Models.Dtos;

namespace App.Db;

public class MarketDbQueries : Domain.Interfaces.Database.Queries.IMarketDbQueries
{
    public Task<OrderDto> GetOrderAsync(CreateOrderDto order)
    {
        throw new NotImplementedException();
    }
}