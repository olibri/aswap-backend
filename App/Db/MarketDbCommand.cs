using App.Mapper;
using Domain.Models.Dtos;
using Domain.Models.Events;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

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

    public async Task UpdateCurrentOfferAsync(UpdateOrderDto updateOrder)
    {
        var entity = await dbContext.EscrowOrders
            .FirstOrDefaultAsync(x => x.DealId == updateOrder.OrderId);

        if (entity == null)
        {
            throw new InvalidOperationException($"EscrowOrderEntity with DealId {updateOrder.OrderId} was not found.");
        }

        entity.MaxFiatAmount = updateOrder.MaxFiatAmount;
        entity.MinFiatAmount = updateOrder.MinFiatAmount;

        dbContext.EscrowOrders.Update(entity);
        await dbContext.SaveChangesAsync();
    }
}