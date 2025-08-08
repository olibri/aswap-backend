using App.Services.QuerySpec.Realization;
using Domain.Enums;
using Domain.Models.Api.QuerySpecs;
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


  public async Task<EscrowOrderDto[]> GetAllNewOffersAsync(
    OffersQuery q, CancellationToken ct = default)
  {
    var baseQ = dbContext.EscrowOrders
      .Where(o => o.EscrowStatus == EscrowStatus.PendingOnChain
                  || o.EscrowStatus == EscrowStatus.OnChain
                  || o.EscrowStatus == EscrowStatus.PartiallyOnChain);

    var spec = q.BuildSpec();
    var page = await spec.ExecuteAsync(baseQ.AsNoTracking());

    var zz = page.Data.Count;
    Console.WriteLine($"Offers found: {zz}");
    return page.Data.Select(EscrowOrderDto.FromEntity).ToArray();
  }


  public Task<EscrowOrderDto[]> GetAllAdminOffersAsync()
  {
    return dbContext.EscrowOrders
      .Where(o => o.EscrowStatus == EscrowStatus.AdminResolving)
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