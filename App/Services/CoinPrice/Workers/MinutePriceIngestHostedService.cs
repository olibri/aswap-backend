using Domain.Interfaces.Services.CoinService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Services.CoinPrice.Workers;

public sealed class MinutePriceIngestHostedService(
  IPriceIngestFacade facade,
  ILogger<MinutePriceIngestHostedService> log,
  TimeProvider timeProvider)
  : BackgroundService
{
  private readonly TimeSpan _period = TimeSpan.FromMinutes(5);

  protected override async Task ExecuteAsync(CancellationToken ct)
  {
    await AlignToNext5Min(ct);

    while (!ct.IsCancellationRequested)
    {
      var started = timeProvider.GetUtcNow();
      try
      {
        await facade.PollPricesAsync(ct);
      }
      catch (Exception ex)
      {
        log.LogError(ex, "Price ingest iteration failed");
      }

      var delay = _period - (timeProvider.GetUtcNow() - started);
      if (delay < TimeSpan.FromSeconds(1)) delay = TimeSpan.FromSeconds(1);
      await Task.Delay(delay, timeProvider, ct);
    }
  }

  private async Task AlignToNext5Min(CancellationToken ct)
  {
    var now = timeProvider.GetUtcNow();
    var mins = now.Minute / 5 * 5;
    var next = new DateTime(now.Year, now.Month, now.Day, now.Hour, mins, 0, DateTimeKind.Utc)
      .AddMinutes(5);
    await Task.Delay(next - now, timeProvider, ct);
  }
}