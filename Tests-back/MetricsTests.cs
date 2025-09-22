using App.Metrics.TaskMetrics;
using App.Utils;
using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.DB.Metrics;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.Offers;

namespace Tests_back;

public class MetricsTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public void TvlUtils_Calculates_Correctly()
  {
    var expectedTvl = 130m;
    var orders = new[]
    {
      new EscrowOrderEntity
        { TokenMint = "USDc", Status = UniversalOrderStatus.Active, Amount = 100, FilledQuantity = 0 },
      new EscrowOrderEntity
        { TokenMint = "USDc", Status = UniversalOrderStatus.Active, Amount = 50, FilledQuantity = 20 },
      new EscrowOrderEntity { TokenMint = "BONK", Status = UniversalOrderStatus.Cancelled, Amount = 99 }
    };

    var tvl = TvlUtils.Calculate(orders);

    tvl["USDc"].ShouldBe(expectedTvl);
    tvl.ContainsKey("BONK").ShouldBeFalse();
  }

  [Fact]
  public async Task Tvl_Is_Saved_To_Snapshot()
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var seeded = await db.SeedAsync(
      50,
      UniversalOrderStatus.Active,
      "USDc",
      OrderSide.Sell,
      fixedAmount: 100);

    var factory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
    var task = new TvlSnapshotTask(factory);

    await task.ExecuteAsync(CancellationToken.None);

    var snap = await db.TvlSnapshots
      .Where(s => s.TokenMint == "USDc")
      .OrderByDescending(s => s.TakenAt)
      .FirstAsync();

    var expected = seeded.ExpectedTvl("USDc");
    snap.Balance.ShouldBe(expected);
  }

  [Fact]
  public async Task OrderStatusSnapshot_Writes_Correct_Counts()
  {
    PostgresDatabase.ResetState("escrow_orders");

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    await db.SeedAsync(10, UniversalOrderStatus.Active, "USDc", OrderSide.Buy, fixedAmount: 100);
    await db.SeedAsync(3, UniversalOrderStatus.Active, "USDc", OrderSide.Buy, fixedAmount: 100);
    await db.SeedAsync(7, UniversalOrderStatus.Completed, "USDc", OrderSide.Sell, fixedAmount: 100);
    await db.SeedAsync(5, UniversalOrderStatus.Cancelled, "USDc", OrderSide.Sell, fixedAmount: 100);

    var factory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
    var task = new OrderStatusSnapshotTask(factory);
    await task.ExecuteAsync(CancellationToken.None);

    var day = DateTime.UtcNow.Date;
    var snaps = await db.OrderStatusDaily
      .Where(s => s.Day == day)
      .ToDictionaryAsync(s => s.Status, s => s.Cnt);

    snaps[UniversalOrderStatus.Active].ShouldBe(13); // 10 + 3
    snaps[UniversalOrderStatus.Completed].ShouldBe(7);
    snaps[UniversalOrderStatus.Cancelled].ShouldBe(5);
  }

  [Fact]
  public async Task TradeMetricsTask_Writes_Correct_Metrics()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("tvl_snapshots");
    PostgresDatabase.ResetState("account");
    PostgresDatabase.ResetState("rooms");
    PostgresDatabase.ResetState("messages");
    PostgresDatabase.ResetState("telegram_link");
    PostgresDatabase.ResetState("outbox_messages");
    PostgresDatabase.ResetState("events");
    PostgresDatabase.ResetState("order_created_daily");
    PostgresDatabase.ResetState("order_status_daily");
    PostgresDatabase.ResetState("asset_volume_daily");
    PostgresDatabase.ResetState("deal_time_daily");
    PostgresDatabase.ResetState("user_metrics_daily");

    await using var scope = fixture.Host.Services.CreateAsyncScope();

    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var settled = DateTime.UtcNow;
    var orders = await db.SeedTradesAsync(3, settled);
    var exp = orders.ExpectedTradeMetrics(settled);

    var task = new TradeMetricsTask(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());
    await task.ExecuteAsync(CancellationToken.None);

    var dtd = await db.Set<DealTimeDailyEntity>()
      .FirstAsync(x => x.Day == settled.Date && x.TokenMint == "USDc");
    var vol = await db.Set<AssetVolumeDailyEntity>()
      .FirstAsync(x => x.Day == settled.Date && x.TokenMint == "USDc");

    dtd.TradeCnt.ShouldBe(exp.Trades);
    dtd.AvgSeconds.ShouldBe(exp.AvgSec, 0.1);
    vol.Volume.ShouldBe(exp.Volume);
  }

  [Fact]
  public async Task UserMetricsDailyTask_Writes_Correct_Snapshot()
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    PostgresDatabase.ResetState("account");
    PostgresDatabase.ResetState("sessions");
    PostgresDatabase.ResetState("user_metrics_daily");

    var exp = await db.SeedUserMetricsScenarioAsync(
      5,
      3,
      2,
      4);

    var task = new UserMetricsDailyTask(scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());
    await task.ExecuteAsync(CancellationToken.None);

    var today = DateTime.UtcNow.Date;
    var snap = await db.UserMetricsDaily.FirstAsync(x => x.Day == today);

    snap.DauUsers.ShouldBe(exp.DauUsers);
    snap.DauIps.ShouldBe(exp.DauIps);
    snap.WauUsers.ShouldBe(exp.WauUsers);
    snap.MauUsers.ShouldBe(exp.MauUsers);
  }

  [Fact]
  public async Task Session_Start_creates_row_and_updates_account()
  {
    PostgresDatabase.ResetState("sessions");
    PostgresDatabase.ResetState("account");

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();
    var pingController = fixture.GetService<SessionPingController>();

    var dto = await db.SeedAccountAsync("w1");
    var before = DateTime.UtcNow;
    await pingController.Start(dto, CancellationToken.None);

    var sess = await db.Sessions.FirstAsync();
    sess.SessionId.ShouldBe(dto.SessionId);
    sess.Wallet.ShouldBe("w1");

    var acc = await db.Account.AsNoTracking().FirstAsync();
    var diff = acc.LastActiveTime!.Value - before;

    diff.ShouldBeLessThan(TimeSpan.FromHours(1));
  }

  [Fact]
  public async Task Session_Ping_updates_last_seen_only()
  {
    PostgresDatabase.ResetState("sessions");
    PostgresDatabase.ResetState("account");

    var ping = fixture.GetService<SessionPingController>();

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var dto = await db.SeedAccountAsync("w2");
    await ping.Start(dto, CancellationToken.None);

    var before = await db.Sessions
      .AsNoTracking()
      .SingleAsync(s => s.SessionId == dto.SessionId);

    await Task.Delay(1500);
    await ping.Ping(dto, CancellationToken.None);

    var after = await db.Sessions
      .AsNoTracking()
      .SingleAsync(s => s.SessionId == dto.SessionId);

    after.StartedAt.ShouldBe(before.StartedAt);

    after.LastSeenAt.ShouldBeGreaterThan(before.LastSeenAt);
  }

  [Fact]
  public async Task AdminDashboard_Returns_Expected_Snapshot_For_Today()
  {
    fixture.ResetDb(
      "escrow_orders", "tvl_snapshots", "asset_volume_daily", "deal_time_daily",
      "user_metrics_daily", "sessions", "events", "account");

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var tvl1 = await db.SeedAsync(1, UniversalOrderStatus.Active, "USDc", OrderSide.Sell, fixedAmount: 100);
    var tvl2 = await db.SeedAsync(1, UniversalOrderStatus.Active, "USDc", OrderSide.Sell, partialFill: true, fixedAmount: 50);
    var tvlOrders = tvl1.Concat(tvl2).ToList();

    var settled = DateTime.UtcNow;
    var trades = await db.SeedTradesAsync(3, settled, "USDc", qtyEach: 100m, priceFiat: 1m);
    var expTrade = trades.ExpectedTradeMetrics(settled);

    var expUsers = await db.SeedUserMetricsScenarioAsync(dauUsers: 5, wauExtra: 2, mauExtra: 4, dauIps: 3);

    await fixture.RunTvlSnapshotAsync();
    await fixture.RunTradeMetricsAsync();
    await fixture.RunUserMetricsDailyAsync();

    fixture.ResetDb("sessions");
    await fixture.SeedOnlineSessionsAsync(online: 2, stale: 1);

    var today = DateTime.UtcNow.Date;
    var dto = await fixture.ExecuteDashboardAsync(from: today, to: today);

    dto.Tvl7d.AssertLast("USDc", tvlOrders.ExpectedTvl("USDc"));
    dto.OrdersSummary["Completed"].ShouldBe(3);                          
    dto.VolumeByAsset["USDc"].ShouldBe(expTrade.Volume);                        
    dto.AvgDealTimeSec!.Value.ShouldBe(expTrade.AvgSec, 0.6);  
    dto.Users.Dau.ShouldBe(expUsers.DauUsers);                                  
    dto.Ips.Dau.ShouldBe(expUsers.DauIps);                                      
    dto.OnlineUsers.ShouldBeGreaterThanOrEqualTo(2);                    
  }

  [Fact]
  public async Task AdminDashboard_Respects_Date_Range_AsOf_To()
  {
    fixture.ResetDb("tvl_snapshots", "user_metrics_daily", "sessions", "escrow_orders", "events",
    "asset_volume_daily", "deal_time_daily");

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var yesterday = DateTime.UtcNow.Date.AddDays(-1);
    var today = DateTime.UtcNow.Date;

    await db.TvlSnapshots.AddRangeAsync(
      new TvlSnapshotEntity { TokenMint = "USDc", Balance = 10m, TakenAt = yesterday.AddHours(10) },
      new TvlSnapshotEntity { TokenMint = "USDc", Balance = 99m, TakenAt = today.AddHours(10) }
    );
    await db.SaveChangesAsync();

    var dtoAsOfYesterday = await fixture.ExecuteDashboardAsync(yesterday, yesterday);
    dtoAsOfYesterday.Tvl7d.AssertLast("USDc", 10m);

    var dtoAsOfToday = await fixture.ExecuteDashboardAsync(today, today);
    dtoAsOfToday.Tvl7d.AssertHasPoint("USDc", yesterday, 10m); 
    dtoAsOfToday.Tvl7d.AssertLast("USDc", 99m);
  }
}