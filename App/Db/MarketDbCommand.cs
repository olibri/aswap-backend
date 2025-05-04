using App.Mapper;
using Domain.Models.Events;
using Infrastructure;

namespace App.Db;

public class MarketDbCommand(P2PDbContext dbContext) : Domain.Interfaces.Database.Command.IMarketDbCommand
{
    public async Task CreateNewOfferAsync(OfferInitialized offer)
    {
        try
        {
            var mappedEntity = EscrowOrderMapper.ToEntity(offer);
            await dbContext.EscrowOrders.AddAsync(mappedEntity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}