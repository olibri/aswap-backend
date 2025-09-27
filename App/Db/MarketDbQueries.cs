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

    var items = page.Data.Select(EscrowOrderDto.FromEntity).ToList();
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