namespace App.Db;

public class MarketDbCommand : Domain.Interfaces.Database.Command.IMarketDbCommand
{
    public Task<Domain.Models.Dtos.OrderDto> CreateOrderAsync(Domain.Models.Dtos.CreateOrderDto order)
    {
        throw new NotImplementedException();
    }
}