using Domain.Enums;
using Domain.Interfaces.Services.Order;
using Domain.Models.Api.Order;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Order;

public sealed class BestPriceService(IDbContextFactory<P2PDbContext> dbf) : IBestPriceService
{
  public async Task<BestPriceDto?> GetBestPriceAsync(BestPriceRequest req, CancellationToken ct)
  {
    await using var db = await dbf.CreateDbContextAsync(ct);

    var q = BuildInitialQuery(db, req);
    q = ApplyPriceSorting(q, req.Side);
    var res = await MapToBestPriceDto(q).FirstOrDefaultAsync(ct);

    return res;
  }

  private static IQueryable<EscrowOrderEntity> BuildInitialQuery(P2PDbContext db, BestPriceRequest req)
  {
    return db.EscrowOrders
      .AsNoTracking()
      .Where(o =>
        o.TokenMint == req.TokenMint &&
        o.FiatCode == req.FiatCode &&
        o.OfferSide == req.Side &&
        (o.EscrowStatus == EscrowStatus.OnChain || o.EscrowStatus == EscrowStatus.PartiallyOnChain));
  }


  private static IOrderedQueryable<EscrowOrderEntity> ApplyPriceSorting(
    IQueryable<EscrowOrderEntity> q, OrderSide side)
  {
    return side == OrderSide.Sell
      ? q.OrderBy(o => o.Price)
      : q.OrderByDescending(o => o.Price);
  }

  private static IQueryable<BestPriceDto> MapToBestPriceDto(IQueryable<EscrowOrderEntity> q)
  {
    return q.Select(o => new BestPriceDto
    {
      OrderId = o.Id,
      Side = o.OfferSide,
      Price = o.Price / 100,
    });
  }
}