using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jobs;
using Domain.Interfaces.Services.CoinService.TokenRepo;

namespace App.Services.CoinPrice.Jobs;

public sealed class DailyPriceRetentionJob(IPriceSnapshotRepository repo, IAppLockService @lock)
  : IDailyPriceRetentionJob
{
  public async Task RunOnceAsync(CancellationToken ct)
  {
    await using var handle = await @lock.TryAcquireAsync("daily-price-retention", ct);
    if (handle is null) return;

    var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
    await repo.DeleteAllExceptDayAsync(todayUtc, ct);
  }
}