using App.Services.QuerySpec.Realization;
using Domain.Enums;
using Domain.Models.Api.Order;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.DB;
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


  public async Task<PagedResult<EscrowOrderDto>> GetAllNewOffersAsync(
    OffersQuery q, CancellationToken ct = default)
  {
    var qDb = q with { PriceFrom = q.PriceFrom * 100m };

    var baseQ = dbContext.EscrowOrders
      .Where(o => o.EscrowStatus == EscrowStatus.PendingOnChain
                  || o.EscrowStatus == EscrowStatus.OnChain
                  || o.EscrowStatus == EscrowStatus.PartiallyOnChain)
      .Include(o => o.PaymentMethods)
      .ThenInclude(link => link.Method)
      .ThenInclude(m => m.Category)
      .AsSplitQuery()
      .AsNoTracking();

    var spec = qDb.BuildSpec();
    var page = await spec.ExecuteAsync(baseQ);

    var items = page.Data.Select(EscrowOrderDto.FromEntity).ToList();
    return new PagedResult<EscrowOrderDto>(items, page.Page, page.Size, page.Total);
  }


  public Task<EscrowOrderDto[]> GetAllAdminOffersAsync()
  {
    return dbContext.EscrowOrders
      .Where(o => o.EscrowStatus == EscrowStatus.AdminResolving)
      .OrderByDescending(o => o.CreatedAtUtc)
      .Select(o => EscrowOrderDto.FromEntity(o))
      .ToArrayAsync();
  }


  public async Task<PagedResult<EscrowOrderDto>> GetAllUsersOffersAsync(string userId, UserOffersQuery q)
  {
    var baseQ = dbContext.EscrowOrders
      .Where(e => e.BuyerFiat == userId || e.SellerCrypto == userId)
      .Include(o => o.PaymentMethods)
      .ThenInclude(link => link.Method)
      .ThenInclude(m => m.Category)
      .AsSplitQuery()
      .AsNoTracking(); 

    var spec = q.BuildSpec();
    var page = await spec.ExecuteAsync(baseQ);

    var dealIds = page.Data.Select(e => e.DealId).Distinct().ToArray();

    // Одним запитом тягнемо всіх дітей цих дилів
    var children = await dbContext.Set<ChildOrderEntity>()
      .AsNoTracking()
      .Where(c => dealIds.Contains(c.DealId))
      .OrderByDescending(c => c.CreatedAtUtc)
      .Select(c => new ChildOrderDto(
        c.Id,
        c.ParentOrderId,
        c.DealId,
        c.OrderOwnerWallet,
        c.ContraAgentWallet,
        c.EscrowStatus,
        c.CreatedAtUtc,
        c.ClosedAtUtc,
        c.FilledAmount,
        c.FillNonce,
        c.FillPda))
      .ToListAsync();


    var byDeal = children
      .GroupBy(c => c.DealId)
      .ToDictionary(g => g.Key, g => g.ToList());

    var items = page.Data.Select(e =>
    {
      var dto = EscrowOrderDto.FromEntity(e);
      if (byDeal.TryGetValue(e.DealId, out var kids))
        dto.Children = kids;
      else
        dto.Children = new List<ChildOrderDto>();

      return dto;
    }).ToList(); return new PagedResult<EscrowOrderDto>(items, page.Page, page.Size, page.Total);
  }

  public async Task<EscrowOrderDto?> CheckOrderStatusAsync(ulong orderId)
  {
    return await dbContext.EscrowOrders
      .Where(x => x.DealId == orderId)
      .Select(o => EscrowOrderDto.FromEntity(o))
      .FirstOrDefaultAsync();
  }
}