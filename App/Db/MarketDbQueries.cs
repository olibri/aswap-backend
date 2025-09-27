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
  public async Task<EscrowOrderDto> GetNewOfferAsync(ulong orderId)
  {
    var order = await dbContext.EscrowOrders.FirstOrDefaultAsync(x => x.OrderId == orderId);

    if (order == null)
      throw new InvalidOperationException($"No order found with orderId: {orderId}");

    return EscrowOrderDto.FromEntity(order);
  }

  public async Task<PagedResult<EscrowOrderDto>> GetAllNewOffersAsync(
    OffersQuery q, CancellationToken ct = default)
  {
    var qDb = q with { PriceFrom = q.PriceFrom * 100m };

    var baseQ = dbContext.EscrowOrders
      .Where(o => o.Status == UniversalOrderStatus.Created
                  || o.Status == UniversalOrderStatus.Active)
      .Include(o => o.PaymentMethods)
      .ThenInclude(link => link.Method)
      .ThenInclude(m => m.Category)
      .AsSplitQuery()
      .AsNoTracking();

    var spec = qDb.BuildSpec();
    var page = await spec.ExecuteAsync(baseQ);

    // map first
    var items = page.Data.Select(EscrowOrderDto.FromEntity).ToList();

    // batch-load stats for unique creators on the page
    var creators = page.Data
      .Select(e => e.CreatorWallet)
      .Where(w => !string.IsNullOrWhiteSpace(w))
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToArray();

    if (creators.Length > 0)
    {
      var creatorAgg = dbContext.EscrowOrders
        .AsNoTracking()
        .Where(o => o.CreatorWallet != null && creators.Contains(o.CreatorWallet))
        .Select(o => new { User = o.CreatorWallet!, o.Status });

      var acceptorAgg = dbContext.EscrowOrders
        .AsNoTracking()
        .Where(o => o.AcceptorWallet != null && creators.Contains(o.AcceptorWallet))
        .Select(o => new { User = o.AcceptorWallet!, o.Status });

      var agg = await creatorAgg.Concat(acceptorAgg)
        .GroupBy(x => x.User)
        .Select(g => new
        {
          User = g.Key,
          Total = g.Count(),
          Completed = g.Count(x => x.Status == UniversalOrderStatus.Completed)
        })
        .ToListAsync(ct);

      var byUser = agg.ToDictionary(
        x => x.User,
        x => (Total: x.Total, SuccessPct: x.Total == 0 ? 0m : Math.Round((decimal)x.Completed / x.Total * 100m, 2)),
        StringComparer.OrdinalIgnoreCase);

      foreach (var dto in items)
      {
        if (!string.IsNullOrWhiteSpace(dto.CreatorWallet) &&
            byUser.TryGetValue(dto.CreatorWallet, out var m))
        {
          dto.UserOrdersCount = m.Total;
          dto.UserSuccessRatePercent = m.SuccessPct;
        }
      }
    }

    return new PagedResult<EscrowOrderDto>(items, page.Page, page.Size, page.Total);
  }

  public Task<EscrowOrderDto[]> GetAllAdminOffersAsync()
  {
    return dbContext.EscrowOrders
      .Where(o => o.Status == UniversalOrderStatus.AdminResolving)
      .OrderByDescending(o => o.CreatedAtUtc)
      .Select(o => EscrowOrderDto.FromEntity(o))
      .ToArrayAsync();
  }

  public async Task<PagedResult<EscrowOrderDto>> GetAllUsersOffersAsync(string userId, UserOffersQuery q)
  {
    var baseQ = dbContext.EscrowOrders
      .Where(e => e.AcceptorWallet == userId || e.CreatorWallet == userId)
      .Include(o => o.PaymentMethods)
      .ThenInclude(link => link.Method)
      .ThenInclude(m => m.Category)
      .Include(o => o.ChildOrders)
      .AsSplitQuery()
      .AsNoTracking();

    var spec = q.BuildSpec();
    var page = await spec.ExecuteAsync(baseQ);

    var orderIds = page.Data.Select(e => e.Id).Distinct().ToArray();

    var children = await dbContext.Set<UniversalTicketEntity>()
      .AsNoTracking()
      .Where(c => orderIds.Contains(c.ParentOrderId))
      .OrderByDescending(c => c.CreatedAtUtc)
      .ToListAsync();

    var byOrderId = children
      .GroupBy(c => c.ParentOrderId)
      .ToDictionary(g => g.Key,
        g => g.Select(ChildOrderDto.FromEntity).ToList());

    var items = page.Data.Select(e =>
    {
      var dto = EscrowOrderDto.FromEntity(e);
      if (byOrderId.TryGetValue(e.Id, out var kids))
        dto.Children = kids;
      else
        dto.Children = new List<ChildOrderDto>();

      return dto;
    }).ToList();

    // aggregate once for the requested user (lifetime)
    var totals = await dbContext.EscrowOrders
      .AsNoTracking()
      .Where(o => o.CreatorWallet == userId || o.AcceptorWallet == userId)
      .GroupBy(_ => 1)
      .Select(g => new
      {
        Total = g.Count(),
        Completed = g.Count(o => o.Status == UniversalOrderStatus.Completed)
      })
      .FirstOrDefaultAsync();

    var total = totals?.Total ?? 0;
    var successPct = total == 0 ? 0m : Math.Round((decimal)(totals!.Completed) / total * 100m, 2);

    foreach (var dto in items)
    {
      dto.UserOrdersCount = total;
      dto.UserSuccessRatePercent = successPct;
    }

    return new PagedResult<EscrowOrderDto>(items, page.Page, page.Size, page.Total);
  }

  public async Task<EscrowOrderDto?> CheckOrderStatusAsync(ulong orderId)
  {
    return await dbContext.EscrowOrders
      .Where(x => x.OrderId == orderId)
      .Select(o => EscrowOrderDto.FromEntity(o))
      .FirstOrDefaultAsync();
  }
}