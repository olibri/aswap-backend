using App.Metrics.TaskMetrics;
using App.Utils;
using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Interfaces.Database.Queries;
using Domain.Models.DB;
using Domain.Models.DB.Metrics;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class MetricsTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public void TvlUtils_Calculates_Correctly()
  {
    var expectedTvl = 130m;
    var orders = new[]
    {
      new EscrowOrderEntity { TokenMint = "USDc", Status = EscrowStatus.OnChain, Amount = 100, FilledQuantity = 0 },
      new EscrowOrderEntity
        { TokenMint = "USDc", Status = EscrowStatus.PartiallyOnChain, Amount = 50, FilledQuantity = 20 },
      new EscrowOrderEntity { TokenMint = "BONK", Status = EscrowStatus.Cancelled, Amount = 99 }
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
      EscrowStatus.OnChain,
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

    await db.SeedAsync(10, EscrowStatus.OnChain, "USDc", OrderSide.Buy, fixedAmount: 100);
    await db.SeedAsync(3, EscrowStatus.PartiallyOnChain, "USDc", OrderSide.Buy, fixedAmount: 100);
    await db.SeedAsync(7, EscrowStatus.Released, "USDc", OrderSide.Sell, fixedAmount: 100);
    await db.SeedAsync(5, EscrowStatus.Cancelled, "USDc", OrderSide.Sell, fixedAmount: 100);

    var factory = scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
    var task = new OrderStatusSnapshotTask(factory);
    await task.ExecuteAsync(CancellationToken.None);

    var day = DateTime.UtcNow.Date;
    var snaps = await db.OrderStatusDaily
      .Where(s => s.Day == day)
      .ToDictionaryAsync(s => s.Status, s => s.Cnt);

    snaps[EscrowStatus.OnChain].ShouldBe(10);
    snaps[EscrowStatus.PartiallyOnChain].ShouldBe(3);
    snaps[EscrowStatus.Released].ShouldBe(7);
    snaps[EscrowStatus.Cancelled].ShouldBe(5);
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

    var pingController = fixture.GetService<SessionPingController>();

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var dto = await db.SeedAccountAsync("w2");
    await pingController.Start(dto, CancellationToken.None);


    var createdAt = await db.Sessions
      .Where(s => s.SessionId == dto.SessionId)
      .Select(s => s.StartedAt)
      .FirstAsync();

    await Task.Delay(1000);
    await pingController.Ping(dto, CancellationToken.None);

    var sess = await db.Sessions.FirstAsync();
    sess.StartedAt.ShouldBe(createdAt);                   
    sess.LastSeenAt.ShouldBeGreaterThan(createdAt);        
  }

}