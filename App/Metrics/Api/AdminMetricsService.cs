using App.Services.QuerySpec.Realization.Helpers;
using Domain.Interfaces.Metrics;
using Domain.Models.Api.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Metrics.Api;

public sealed class AdminMetricsService(P2PDbContext db) : IAdminMetricsService
{
  private static readonly TimeSpan OnlineWindow = TimeSpan.FromSeconds(90);

  public async Task<DashboardMetricsDto> GetDashboardAsync(DashboardQuery q, CancellationToken ct)
  {
    var (from, to) = q.Normalize();
    var nowUtc = DateTime.UtcNow;

    var tvlByAsset = await LoadLatestTvlByAssetAsOfAsync(to, ct);
    var ordersSummary = await LoadOrdersSummaryNowAsync(ct);
    var avgDealTimeSec = await LoadWeightedAvgDealTimeAsync(from, to, ct);
    var volumeByAsset = await LoadVolumeByAssetAsync(from, to, ct);
    var (users, ips) = await LoadUserMetricsSnapshotAsOfAsync(to, ct);
    var onlineUsersCount = await CountOnlineUsersAsync(nowUtc, OnlineWindow, ct);

    return new DashboardMetricsDto(
      tvlByAsset,
      ordersSummary,
      avgDealTimeSec,
      volumeByAsset,
      users,
      ips,
      onlineUsersCount
    );
  }

  private async Task<IReadOnlyDictionary<string, decimal>> LoadLatestTvlByAssetAsOfAsync(DateTime asOf,
    CancellationToken ct)
  {
    var range = new DateRangeFilter<TvlSnapshotEntity>(s => s.TakenAt, DateTime.MinValue, asOf);
    var filtered = range.Apply(db.TvlSnapshots).AsNoTracking();
    var toExclusive = EndOfDayExclusiveUtc(asOf);
    var rows = await db.TvlSnapshots
      .AsNoTracking()
      .Where(s => s.TakenAt < toExclusive)         // < (exclusive)
      .GroupBy(s => s.TokenMint)
      .Select(g => g.OrderByDescending(x => x.TakenAt).First())
      .ToListAsync(ct);

    return rows.ToDictionary(s => s.TokenMint, s => s.Balance, StringComparer.OrdinalIgnoreCase);
  }
  static DateTime EndOfDayExclusiveUtc(DateTime d) => d.Date.AddDays(1);


  private async Task<IReadOnlyDictionary<string, int>> LoadOrdersSummaryNowAsync(CancellationToken ct)
  {
    var rows = await db.EscrowOrders
      .AsNoTracking()
      .GroupBy(o => o.EscrowStatus)
      .Select(g => new { Key = g.Key.ToString(), Cnt = g.Count() })
      .ToListAsync(ct);

    return rows.ToDictionary(x => x.Key, x => x.Cnt, StringComparer.OrdinalIgnoreCase);
  }

  private async Task<double?> LoadWeightedAvgDealTimeAsync(DateTime from, DateTime to, CancellationToken ct)
  {
    var filter = new DateRangeFilter<DealTimeDailyEntity>(x => x.Day, from, to);
    var rows = await filter.Apply(db.Set<DealTimeDailyEntity>())
      .AsNoTracking()
      .Select(x => new { x.TradeCnt, x.AvgSeconds })
      .ToListAsync(ct);

    var totalTrades = rows.Sum(x => x.TradeCnt);
    if (totalTrades == 0) return null;

    return rows.Sum(x => x.AvgSeconds * x.TradeCnt) / totalTrades;
  }

  private async Task<IReadOnlyDictionary<string, decimal>> LoadVolumeByAssetAsync(DateTime from, DateTime to,
    CancellationToken ct)
  {
    var filter = new DateRangeFilter<AssetVolumeDailyEntity>(v => v.Day, from, to);
    var rows = await filter.Apply(db.Set<AssetVolumeDailyEntity>())
      .AsNoTracking()
      .GroupBy(v => v.TokenMint)
      .Select(g => new { Asset = g.Key, Volume = g.Sum(x => x.Volume) })
      .ToListAsync(ct);

    return rows.ToDictionary(x => x.Asset, x => x.Volume, StringComparer.OrdinalIgnoreCase);
  }

  private async Task<(UsersSnapshot Users, IpsSnapshot Ips)> LoadUserMetricsSnapshotAsOfAsync(DateTime asOf,
    CancellationToken ct)
  {
    var filter = new DateRangeFilter<UserMetricsDailyEntity>(x => x.Day, DateTime.MinValue, asOf);
    var snap = await filter.Apply(db.UserMetricsDaily)
      .AsNoTracking()
      .OrderByDescending(x => x.Day)
      .FirstOrDefaultAsync(ct);

    var users = snap is null
      ? new UsersSnapshot(0, 0, 0)
      : new UsersSnapshot(snap.DauUsers, snap.WauUsers, snap.MauUsers);

    var ips = snap is null
      ? new IpsSnapshot(0)
      : new IpsSnapshot(snap.DauIps);

    return (users, ips);
  }

  private async Task<int> CountOnlineUsersAsync(DateTime nowUtc, TimeSpan window, CancellationToken ct)
  {
    var cutoff = nowUtc - window;
    return await db.Sessions
      .AsNoTracking()
      .CountAsync(s => s.LastSeenAt >= cutoff, ct);
  }
}