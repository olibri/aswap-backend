﻿using Domain.Enums;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Db;

public class MarketDbQueries(P2PDbContext dbContext) : Domain.Interfaces.Database.Queries.IMarketDbQueries
{
    public async Task<EscrowOrderDto> GetNewOfferAsync(ulong dealId)
    {
        var order = await dbContext.EscrowOrders.FirstOrDefaultAsync(x => x.DealId == dealId);

        if (order == null)
            throw new InvalidOperationException($"No order found with dealId: {dealId}");

        return EscrowOrderDto.FromEntity(order);
    }

    public async Task<EscrowOrderDto[]> GetAllNewOffersAsync()
    {
        return await dbContext.EscrowOrders
            .Where(o => o.Status == EscrowStatus.PendingOnChain
                        || o.Status == EscrowStatus.OnChain || o.Status == EscrowStatus.PartiallyOnChain)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => EscrowOrderDto.FromEntity(o))
            .ToArrayAsync();
    }


    public Task<EscrowOrderDto[]> GetAllAdminOffersAsync()
    {
        return dbContext.EscrowOrders
            .Where(o => o.Status == EscrowStatus.AdminResolving)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => EscrowOrderDto.FromEntity(o))
            .ToArrayAsync();
    }

    public async Task<EscrowOrderDto[]> GetAllUsersOffersAsync(string userId)
    {
        return await dbContext.EscrowOrders
            .Where(o => o.BuyerFiat == userId || o.SellerCrypto == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => EscrowOrderDto.FromEntity(o))
            .ToArrayAsync();
    }

    public async Task<EscrowOrderDto?> CheckOrderStatusAsync(ulong orderId)
    {
        return await dbContext.EscrowOrders
            .Where(x => x.DealId == orderId)
            .Select(o => EscrowOrderDto.FromEntity(o))
            .FirstOrDefaultAsync();
    }
}