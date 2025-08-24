using App.Services.CoinPrice.Jobs;
using App.Services.CoinPrice.Workers;
using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.Services.CoinService.Planner;
using Domain.Interfaces.Services.CoinService.TokenRepo;
using Domain.Models.Api.CoinPrice;
using Domain.Models.DB.CoinPrice;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class CoinPriceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task TokenApi_VerifiedTokens_AreReturned_And_Persisted()
  {
    // reset
    PostgresDatabase.ResetState("token");
    var db = fixture.GetService<P2PDbContext>();
    db.ChangeTracker.Clear();
    var tokenClient = fixture.GetService<IJupTokenClient>();
    var tokenRepo = fixture.GetService<ITokenRepository>();
    var locks = fixture.GetService<IAppLockService>();

    var job = new TokenBootstrapJob(tokenRepo, tokenClient, locks);
    await job.RunOnceAsync(default);

    // assert
    var count = await db.Set<TokenEntity>().CountAsync();
    count.ShouldBeGreaterThan(50);
  }

  [Fact]
  public async Task PriceApi_Prices_For_Known_Mints_Are_Positive()
  {
    PostgresDatabase.ResetState("price_snapshot_minute");

    var priceClient = fixture.GetService<IJupPriceClient>();

    var sol = "So11111111111111111111111111111111111111112";
    var usdc = "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v";

    var map = await priceClient.GetUsdPricesAsync(new[] { sol, usdc }, default);

    map.ContainsKey(sol).ShouldBeTrue();
    map.ContainsKey(usdc).ShouldBeTrue();
    map[sol].UsdPrice.ShouldBeGreaterThan(0m);
    map[usdc].UsdPrice.ShouldBeGreaterThan(0m);
  }

  [Fact]
  public async Task Ingest_TwoKnownMints_WritesSnapshots_UpsertWithinSameMinute()
  {
    // reset
    PostgresDatabase.ResetState("token");
    PostgresDatabase.ResetState("price_snapshot_minute");
    PostgresDatabase.ResetState("app_lock");

    var db = fixture.GetService<P2PDbContext>();
    db.ChangeTracker.Clear();
    var tokenRepo = fixture.GetService<ITokenRepository>();
    var priceRepo = fixture.GetService<IPriceSnapshotRepository>();
    var priceClient = fixture.GetService<IJupPriceClient>();
    var planner = fixture.GetService<IPriceBatchPlanner>();
    var locks = fixture.GetService<IAppLockService>();

    await tokenRepo.UpsertAsync(new[]
    {
      new TokenDto(
        "So11111111111111111111111111111111111111112", "SOL", "Solana", 9, true),
      new TokenDto(
        "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", "USDC", "USD Coin", 6, true)
    }, default);

    var cfg = new PriceIngestConfig(
      "USDC",
      5,
      50,
      60,
      TimeSpan.Zero
    );

    var job = new MinutePriceIngestJob(tokenRepo, priceClient, priceRepo, planner, locks, cfg);

    var now = DateTime.UtcNow;
    var bucket = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);

    await job.RunOnceAsync(default);

    var totalAfterFirst = await db.Set<PriceSnapshotEntity>().CountAsync();
    totalAfterFirst.ShouldBeGreaterThanOrEqualTo(2);

    await job.RunOnceAsync(default);

    var inBucket = await db.Set<PriceSnapshotEntity>()
      .Where(x => x.MinuteBucketUtc == bucket)
      .ToListAsync();

    inBucket.Count.ShouldBe(2);
    inBucket.Select(x => x.TokenMint).OrderBy(x => x).ShouldBe(new[]
    {
      "EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v",
      "So11111111111111111111111111111111111111112"
    }.OrderBy(x => x));
  }

  [Fact]
  public Task Planner_Splits_120_Into_50_50_20()
  {
    var planner = fixture.GetService<IPriceBatchPlanner>();
    var cfg = new PriceIngestConfig("USDC", 5, 50, 60, TimeSpan.FromMinutes(1));

    var mints = Enumerable.Range(1, 120).Select(i => $"M{i:D4}").ToList();
    var plan = planner.PlanBatches(mints, cfg);

    plan.Batches.Count.ShouldBe(3);
    plan.Batches[0].Count.ShouldBe(50);
    plan.Batches[1].Count.ShouldBe(50);
    plan.Batches[2].Count.ShouldBe(20);

    plan.DelayBetweenRequests.ShouldBe(TimeSpan.FromSeconds(1));
    return Task.CompletedTask;
  }

  [Fact]
  public async Task Retention_Deletes_Yesterday_Keeps_Today()
  {
    PostgresDatabase.ResetState("token");
    PostgresDatabase.ResetState("price_snapshot_minute");
    PostgresDatabase.ResetState("app_lock");

    var db = fixture.GetService<P2PDbContext>();
    db.ChangeTracker.Clear();
    var repo = fixture.GetService<IPriceSnapshotRepository>();
    var locks = fixture.GetService<IAppLockService>();

    db.Set<TokenEntity>().Add(new TokenEntity { Mint = "TEST_MINT", Symbol = "TST" });
    await db.SaveChangesAsync();

    var today = DateTime.UtcNow.Date;
    var yesterday = today.AddDays(-1);

    var yBucket = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 12, 0, 0, DateTimeKind.Utc);
    var tBucket = new DateTime(today.Year, today.Month, today.Day, 12, 0, 0, DateTimeKind.Utc);

    await repo.UpsertMinuteAsync(new[]
    {
      new PriceSnapshotUpsertDto("TEST_MINT", "USDC", 1m, yBucket, yBucket),
      new PriceSnapshotUpsertDto("TEST_MINT", "USDC", 2m, yBucket.AddMinutes(1), yBucket.AddMinutes(1)),
      new PriceSnapshotUpsertDto("TEST_MINT", "USDC", 3m, tBucket, tBucket)
    }, default);

    var job = new DailyPriceRetentionJob(repo, locks);
    await job.RunOnceAsync(default);

    var left = await db.Set<PriceSnapshotEntity>().AsNoTracking().ToListAsync();
    left.Count.ShouldBe(1);
    left[0].MinuteBucketUtc.Date.ShouldBe(today);
    left[0].Price.ShouldBe(3m);
  }

  [Fact]
  public async Task AppLock_Allows_Single_Holder()
  {
    // reset
    PostgresDatabase.ResetState("app_lock");

    var locks = fixture.GetService<IAppLockService>();

    await using var h1 = await locks.TryAcquireAsync("lock-test", default);
    h1.ShouldNotBeNull();

    var h2 = await locks.TryAcquireAsync("lock-test", default);
    h2.ShouldBeNull();

    await h1.DisposeAsync().ConfigureAwait(false);

    await using var h3 = await locks.TryAcquireAsync("lock-test", default);
    h3.ShouldNotBeNull();
  }


  [Fact]
  public async Task MinuteService_Aligns_To_Next_5_Min_And_Ticks_Every_5_Min()
  {
    var start = new DateTimeOffset(2025, 1, 1, 10, 1, 30, TimeSpan.Zero);
    var tp = new FakeTimeProvider(start);

    var facade = new TestFacade();
    var svc = new MinutePriceIngestHostedService(
      facade,
      NullLogger<MinutePriceIngestHostedService>.Instance,
      tp);

    using var cts = new CancellationTokenSource();
    await svc.StartAsync(cts.Token);

    tp.Advance(TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(30));
    WaitUntil(() => facade.PollCalls == 1, tp);

    tp.Advance(TimeSpan.FromMinutes(5));
    WaitUntil(() => facade.PollCalls == 2, tp);

    tp.Advance(TimeSpan.FromMinutes(5));
    WaitUntil(() => facade.PollCalls == 3, tp);

    await svc.StopAsync(CancellationToken.None);

    static void WaitUntil(Func<bool> cond, FakeTimeProvider tp, int timeoutMs = 1000)
    {
      var sw = System.Diagnostics.Stopwatch.StartNew();
      while (sw.ElapsedMilliseconds < timeoutMs)
      {
        if (cond()) return;
        tp.Advance(TimeSpan.Zero);
        Thread.Yield();
      }

      throw new TimeoutException("Condition not reached in time");
    }
  }

  [Fact]
  public async Task DailyService_Aligns_To_Next_Utc_Midnight_And_Ticks_Daily()
  {
    var start = new DateTimeOffset(2025, 1, 1, 10, 15, 0, TimeSpan.Zero);
    var tp = new FakeTimeProvider(start);

    var facade = new TestFacade();
    var svc = new DailyPriceRetentionHostedService(
      facade,
      NullLogger<DailyPriceRetentionHostedService>.Instance,
      tp);

    using var cts = new CancellationTokenSource();
    await svc.StartAsync(cts.Token);

    tp.Advance(TimeSpan.Zero);

    var nextMidnight = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
    var almost = nextMidnight - start - TimeSpan.FromMilliseconds(1);
    tp.Advance(almost);
    tp.Advance(TimeSpan.Zero);

    tp.Advance(TimeSpan.FromMilliseconds(1));
    WaitUntil(() => facade.CleanupCalls == 1, tp);

    tp.Advance(TimeSpan.FromDays(1));
    WaitUntil(() => facade.CleanupCalls == 2, tp);

    await svc.StopAsync(CancellationToken.None);

    static void WaitUntil(Func<bool> cond, FakeTimeProvider tp, int timeoutMs = 2000)
    {
      var sw = System.Diagnostics.Stopwatch.StartNew();
      while (sw.ElapsedMilliseconds < timeoutMs)
      {
        if (cond()) return;
        tp.Advance(TimeSpan.Zero);
        Thread.Yield();
      }

      throw new TimeoutException("Condition not reached in time");
    }
  }

  [Fact]
  public async Task Concurrency_Ingest_While_Retention_NoDeadlocks_DataConsistent()
  {
    PostgresDatabase.ResetState("token");
    PostgresDatabase.ResetState("price_snapshot_minute");
    PostgresDatabase.ResetState("app_lock");

    var db = fixture.GetService<P2PDbContext>();
    db.ChangeTracker.Clear();

    var tokenRepo = fixture.GetService<ITokenRepository>();
    var priceRepo = fixture.GetService<IPriceSnapshotRepository>();
    var priceCli = fixture.GetService<IJupPriceClient>();
    var planner = fixture.GetService<IPriceBatchPlanner>();
    var locks = fixture.GetService<IAppLockService>();

    await tokenRepo.UpsertAsync(new[]
    {
      new TokenDto("So11111111111111111111111111111111111111112", "SOL", "Solana", 9, true),
      new TokenDto("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", "USDC", "USD Coin", 6, true)
    }, default);

    var today = DateTime.UtcNow.Date;
    var yesterday = today.AddDays(-1);
    var yBucket1 = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, 12, 0, 0, DateTimeKind.Utc);
    var yBucket2 = yBucket1.AddMinutes(1);

    await priceRepo.UpsertMinuteAsync(new[]
    {
      new PriceSnapshotUpsertDto("So11111111111111111111111111111111111111112", "USDC", 1m, yBucket1, yBucket1),
      new PriceSnapshotUpsertDto("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v", "USDC", 1m, yBucket2, yBucket2)
    }, default);

    var cfg = new PriceIngestConfig("USDC", 5, 50, 60, TimeSpan.Zero);
    var ingestJob = new MinutePriceIngestJob(tokenRepo, priceCli, priceRepo, planner, locks, cfg);
    var retentionJob = new DailyPriceRetentionJob(priceRepo, locks);

    for (var i = 0; i < 5; i++)
    {
      using var startBarrier = new Barrier(2);

      var tIngest = Task.Run(async () =>
      {
        startBarrier.SignalAndWait();
        await ingestJob.RunOnceAsync(default);
      });

      var tRetention = Task.Run(async () =>
      {
        startBarrier.SignalAndWait();
        await retentionJob.RunOnceAsync(default);
      });

      await Task.WhenAll(tIngest, tRetention);
    }

    var snapshots = await db.Set<PriceSnapshotEntity>().AsNoTracking().ToListAsync();

    snapshots.Any(s => s.MinuteBucketUtc.Date < today).ShouldBeFalse();

    var todayMints = snapshots
      .Where(s => s.MinuteBucketUtc.Date == today)
      .Select(s => s.TokenMint)
      .Distinct()
      .ToArray();

    todayMints.ShouldContain("So11111111111111111111111111111111111111112");
    todayMints.ShouldContain("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
  }
}