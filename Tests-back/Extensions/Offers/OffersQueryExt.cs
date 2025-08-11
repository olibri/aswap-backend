using App.Metrics.TaskMetrics;
using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Models.Api.Metrics;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.DB.Metrics;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Tests_back.Extensions.Offers;

public static class OffersQueryExt
{
  public static OffersQuery With(
    this OffersQuery q,
    int? page = null, int? size = null,
    OfferSortField? sort = null, SortDir? dir = null,
    EscrowStatus? status = null, OrderSide? side = null,
    string? fiat = null, string? mint = null)
    => q with
    {
      Page = page ?? q.Page,
      Size = size ?? q.Size,
      SortBy = sort ?? q.SortBy,
      Dir = dir ?? q.Dir,
      Status = status ?? q.Status,
      Side = side ?? q.Side,
      Fiat = fiat ?? q.Fiat,
      TokenMint = mint ?? q.TokenMint
    };


  public static async Task RunTvlSnapshotAsync(this TestFixture f, CancellationToken ct = default)
  {
    var factory = f.GetService<IServiceScopeFactory>();
    var task = new TvlSnapshotTask(factory);
    await task.ExecuteAsync(ct);
  }

  public static async Task RunTradeMetricsAsync(this TestFixture f, CancellationToken ct = default)
  {
    var factory = f.GetService<IServiceScopeFactory>();
    var task = new TradeMetricsTask(factory);
    await task.ExecuteAsync(ct);
  }

  public static async Task RunUserMetricsDailyAsync(this TestFixture f, CancellationToken ct = default)
  {
    var factory = f.GetService<IServiceScopeFactory>();
    var task = new UserMetricsDailyTask(factory);
    await task.ExecuteAsync(ct);
  }

  public static async Task<int> SeedOnlineSessionsAsync(this TestFixture f, int online = 1, int stale = 0, CancellationToken ct = default)
  {
    await using var scope = f.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();
    var now = DateTime.UtcNow;

    var list = new List<SessionEntity>();
    for (var i = 0; i < online; i++)
      list.Add(new SessionEntity { Wallet = $"w{i}", Ip = "1.1.1.1", StartedAt = now.AddMinutes(-5), LastSeenAt = now });
    for (var j = 0; j < stale; j++)
      list.Add(new SessionEntity { Wallet = $"s{j}", Ip = "2.2.2.2", StartedAt = now.AddMinutes(-120), LastSeenAt = now.AddMinutes(-15) });

    await db.Sessions.AddRangeAsync(list, ct);
    await db.SaveChangesAsync(ct);
    return list.Count;
  }

  public static async Task<DashboardMetricsDto> ExecuteDashboardAsync(this TestFixture f, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
  {
    var controller = f.GetService<AdminController>();
    var query = new DashboardQuery(From: from, To: to);

    var action = await controller.GetDashboard(query, ct);
    var ok = action.Result as OkObjectResult;
    var dto = action.Value ?? (ok?.Value as DashboardMetricsDto);

    dto.ShouldNotBeNull("Dashboard should return payload");
    return dto!;
  }

  private static long ToUnixDay(DateTime d) =>
    new DateTimeOffset(d.Date, TimeSpan.Zero).ToUnixTimeSeconds();

  public static Series GetSeries(this IReadOnlyList<Series> list, string asset)
  {
    var s = list.FirstOrDefault(x =>
      string.Equals(x.Key, asset, StringComparison.OrdinalIgnoreCase));
    s.ShouldNotBeNull($"TVL series must contain asset '{asset}'");
    return s!;
  }

  public static void AssertLast(this IReadOnlyList<Series> list, string asset, decimal expected)
  {
    var s = list.GetSeries(asset);
    s.Points.Count.ShouldBeGreaterThan(0, $"Series '{asset}' must have at least 1 point");
    s.Points[^1].V.ShouldBe(expected);
  }

  public static void AssertHasPoint(this IReadOnlyList<Series> list, string asset, DateTime day, decimal expected)
  {
    var s = list.GetSeries(asset);
    var ts = ToUnixDay(day);
    var p = s.Points.FirstOrDefault(x => x.T == ts);
    p.ShouldNotBe(default, $"Series '{asset}' should have point for {day:yyyy-MM-dd}");
    p.V.ShouldBe(expected);
  }
}