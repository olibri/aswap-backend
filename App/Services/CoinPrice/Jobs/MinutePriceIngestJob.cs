using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jobs;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.Services.CoinService.Planner;
using Domain.Interfaces.Services.CoinService.TokenRepo;
using Domain.Models.Api.CoinPrice;

namespace App.Services.CoinPrice.Jobs;

public sealed class MinutePriceIngestJob(
  ITokenRepository tokens,
  IJupPriceClient prices,
  IPriceSnapshotRepository repo,
  IPriceBatchPlanner planner,
  IAppLockService @lock,
  PriceIngestConfig cfg)
  : IMinutePriceIngestJob
{
  public async Task RunOnceAsync(CancellationToken ct)
  {
    await using var handle = await @lock.TryAcquireAsync("minute-price-ingest", ct);
    if (handle is null) return;

    var mints = await tokens.GetAllMintsAsync(ct);
    if (mints.Count == 0) return;

    var plan = planner.PlanBatches(mints, cfg);

    var now = DateTime.UtcNow;
    var bucket = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc);

    foreach (var batch in plan.Batches)
    {
      var dict = await prices.GetUsdPricesAsync(batch, ct);

      var rows = dict.Select(kv => new PriceSnapshotUpsertDto(
        TokenMint: kv.Key,
        Quote: cfg.Quote,
        Price: kv.Value.UsdPrice,
        MinuteBucketUtc: bucket,
        CollectedAtUtc: DateTime.UtcNow
      ));

      await repo.UpsertMinuteAsync(rows, ct);

      await Task.Delay(plan.DelayBetweenRequests, ct);
    }
  }
}