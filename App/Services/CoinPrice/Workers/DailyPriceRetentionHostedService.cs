using Domain.Interfaces.Services.CoinService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Services.CoinPrice.Workers;

public sealed class DailyPriceRetentionHostedService(
  IPriceIngestFacade facade,
  ILogger<DailyPriceRetentionHostedService> log,
  TimeProvider timeProvider)
  : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken ct)
  {
    await DelayToNextUtcMidnight(ct);

    while (!ct.IsCancellationRequested)
    {
      try
      {
        await facade.CleanupDailyAsync(ct);
      }
      catch (Exception ex)
      {
        log.LogError(ex, "Daily retention failed");
      }

      await Task.Delay(TimeSpan.FromDays(1), timeProvider, ct);
    }
  }

  private async Task DelayToNextUtcMidnight(CancellationToken ct)
  {
    var now = timeProvider.GetUtcNow();
    var next = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
    await Task.Delay(next - now, timeProvider, ct);               
  }
}