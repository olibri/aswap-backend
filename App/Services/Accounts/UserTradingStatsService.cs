using Domain.Enums;
using Domain.Interfaces.Services.Account;
using Domain.Models.Api.User;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Accounts;

public sealed class UserTradingStatsService(IDbContextFactory<P2PDbContext> dbFactory) : IUserTradingStatsService
{
  public async Task<UserTradingStatsDto> GetUserStatsAsync(string userWallet, CancellationToken ct = default)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var userOrders = db.EscrowOrders
      .AsNoTracking()
      .Where(o => o.SellerCrypto == userWallet || o.BuyerFiat == userWallet);

    var completedOrders = await userOrders
      .Where(o => o.EscrowStatus == EscrowStatus.Released)
      .ToListAsync(ct);

    var totalTrades = completedOrders.Count;

    var buyTradesCount = completedOrders.Count(o =>
      (o.OfferSide == OrderSide.Buy && o.BuyerFiat == userWallet) ||
      (o.OfferSide == OrderSide.Sell && o.SellerCrypto == userWallet));

    var sellTradesCount = totalTrades - buyTradesCount;

    var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
    var tradesLast30Days = completedOrders.Count(o => o.ClosedAtUtc >= thirtyDaysAgo);

    var ordersLast30Days = await userOrders
      .Where(o => o.CreatedAtUtc >= thirtyDaysAgo)
      .ToListAsync(ct);

    var completionRate30D = ordersLast30Days.Count == 0
      ? 0m
      : (decimal)ordersLast30Days.Count(o => o.EscrowStatus == EscrowStatus.Released) / ordersLast30Days.Count * 100m;

    var averageReleaseTime = await CalculateAverageReleaseTimeAsync(db, userWallet, ct);

    var averageTradeTime = CalculateAverageTradeTime(completedOrders);

    var joinedDate = await db.Account
      .Where(a => a.WalletAddress == userWallet)
      .Select(a => a.CreatedAtUtc)
      .FirstOrDefaultAsync(ct);

    return new UserTradingStatsDto(
      totalTrades,
      buyTradesCount,
      sellTradesCount,
      tradesLast30Days,
      Math.Round(completionRate30D, 2),
      Math.Round(averageReleaseTime, 0),
      averageTradeTime,
      joinedDate
    );
  }

  private static async Task<double> CalculateAverageReleaseTimeAsync(
    P2PDbContext db,
    string userWallet,
    CancellationToken ct)
  {
    var releaseTimeSeconds = await db.EscrowOrders
      .AsNoTracking()
      .Where(o => o.SellerCrypto == userWallet &&
                  o.ReleaseTimeSeconds.HasValue)
      .Select(o => o.ReleaseTimeSeconds!.Value)
      .ToListAsync(ct);

    return releaseTimeSeconds.Count == 0 ? 0 : releaseTimeSeconds.Average();
  }

  private static double CalculateAverageTradeTime(List<Domain.Models.DB.EscrowOrderEntity> completedOrders)
  {
    if (completedOrders.Count == 0) return 0;

    var tradesWithTime = completedOrders
      .Where(o => o.ClosedAtUtc.HasValue)
      .Select(o => (o.ClosedAtUtc!.Value - o.CreatedAtUtc).TotalSeconds)
      .ToList();

    return tradesWithTime.Count == 0 ? 0 : tradesWithTime.Average();
  }
}