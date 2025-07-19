using App.Mapper;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Domain.Models.Events;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using Domain.Models.Enums;

namespace App.Db;

public class MarketDbCommand(P2PDbContext dbContext) : IMarketDbCommand
{
    public async Task CreateSellerOfferAsync(OfferInitialized offer)
    {
        try
        {
            var mappedEntity = EscrowOrderMapper.ToEntity(offer);

            mappedEntity.DomainEvents.Add(new OfferCreated(
                Guid.NewGuid(), mappedEntity.Id, mappedEntity.DealId,
                mappedEntity.SellerCrypto, OrderSide.Sell, EventType.OfferCreated));

            await dbContext.EscrowOrders.AddAsync(mappedEntity);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto upsertOrderDto)
    {
        var mappedEntity = EscrowOrderMapper.ToEntity(upsertOrderDto);
        Console.WriteLine($"Creating buyer offer with DealId: {mappedEntity.DealId}");

        mappedEntity.DomainEvents.Add(new OfferCreated(
            Guid.NewGuid(),
            mappedEntity.Id,
            mappedEntity.DealId,
            mappedEntity.BuyerFiat,
            OrderSide.Buy, EventType.OfferCreated));


        await dbContext.EscrowOrders.AddAsync(mappedEntity);
        await dbContext.SaveChangesAsync();
        return mappedEntity.DealId;
    }

    public async Task UpdateCurrentOfferAsync(UpsertOrderDto upsertOrder)
    {
        //TODO: remove this delay, it's just for testing purposes
        await Task.Delay(4000);

        var entity = await dbContext.EscrowOrders
            .FirstOrDefaultAsync(x => x.DealId == upsertOrder.OrderId);

        var allEntitiesInDb = await dbContext.EscrowOrders.ToListAsync();

        Console.WriteLine($"Updating order with DealId: {upsertOrder.OrderId}");
        Console.WriteLine($"Found {allEntitiesInDb.Count} orders in the database.");


        if (entity == null)
            throw new InvalidOperationException($"EscrowOrderEntity with DealId {upsertOrder.OrderId} was not found.");

        entity.MaxFiatAmount = upsertOrder.MaxFiatAmount ?? entity.MaxFiatAmount;
        entity.MinFiatAmount = upsertOrder.MinFiatAmount ?? entity.MinFiatAmount;
        entity.Status = upsertOrder.Status.HasValue
            ? (EscrowStatus)upsertOrder.Status.Value
            : entity.Status;
        entity.BuyerFiat = upsertOrder.Buyer ?? entity.BuyerFiat;
        entity.SellerCrypto = upsertOrder.Seller ?? entity.SellerCrypto;
        if (upsertOrder.FilledQuantity is not null)
        {
            entity.Status = MoveToSignedStatus(entity, (decimal)upsertOrder.FilledQuantity);
            entity.FilledQuantity += (decimal)upsertOrder.FilledQuantity;
        }
        entity.AdminCall = upsertOrder.AdminCall ?? entity.AdminCall;

        dbContext.EscrowOrders.Update(entity);
        await dbContext.SaveChangesAsync();
    }

    private EscrowStatus MoveToSignedStatus(EscrowOrderEntity orderEntity, decimal newFilledQ)
    {
        var fromEntity = EscrowOrderDto.FromEntity(orderEntity);
        if (fromEntity.FilledQuantity + newFilledQ >= fromEntity.Amount)
            return EscrowStatus.Signed;

        return orderEntity.Status;
    }
}