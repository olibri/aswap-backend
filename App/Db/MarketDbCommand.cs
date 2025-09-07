using App.Mapper;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Domain.Models.Events;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using Domain.Models.Enums;
using System.Data;

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

    public async Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto dto)
    {

        var exists = await dbContext.EscrowOrders
            .AsNoTracking()
            .AnyAsync(x => x.DealId == dto.OrderId, CancellationToken.None);
        if (exists)
            throw new InvalidOperationException($"Order with DealId={dto.OrderId} already exists.");

        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, CancellationToken.None);

        var entity = EscrowOrderMapper.ToEntity(dto);
        entity.OfferSide = OrderSide.Buy;
        entity.CreatedAtUtc = DateTime.UtcNow;
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

        entity.DomainEvents.Add(new OfferCreated(
            Guid.NewGuid(),
            entity.Id,
            entity.DealId,
            entity.BuyerFiat,
            OrderSide.Buy,
            EventType.OfferCreated));

        await dbContext.EscrowOrders.AddAsync(entity, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None); 

        var desiredIds = dto.PaymentMethodIds.Distinct().ToArray();
        var validIds = await dbContext.PaymentMethods
            .Where(pm => desiredIds.Contains(pm.Id))
            .Select(pm => pm.Id)
            .ToListAsync(CancellationToken.None);

        if (validIds.Count != desiredIds.Length)
            throw new ValidationException("Some payment methods don’t exist.");

        var links = validIds.Select(id => new EscrowOrderPaymentMethodEntity
        {
            OrderId = entity.Id,
            MethodId = id
        });

        await dbContext.AddRangeAsync(links, CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        await tx.CommitAsync(CancellationToken.None);


        return entity.DealId;

    }

    public async Task UpdateCurrentOfferAsync(UpsertOrderDto upsertOrder)
    {
        //TODO: remove this delay, it's just for testing purposes
        await Task.Delay(4000);

        var entity = await dbContext.EscrowOrders
            .Include(x => x.PaymentMethods)
            .FirstOrDefaultAsync(x => x.DealId == upsertOrder.OrderId);


        if (entity is null)
            throw new InvalidOperationException($"EscrowOrderEntity with DealId {upsertOrder.OrderId} was not found.");

        EscrowOrderPatcher.ApplyUpsert(entity, upsertOrder, MoveToSignedStatus);

        await dbContext.SaveChangesAsync();
    }

    private EscrowStatus MoveToSignedStatus(EscrowOrderEntity orderEntity, decimal newFilledQ)
    {
        var fromEntity = EscrowOrderDto.FromEntity(orderEntity);
        if (fromEntity.FilledQuantity + newFilledQ >= fromEntity.Amount)
            return EscrowStatus.Signed;

        return orderEntity.EscrowStatus;
    }
}