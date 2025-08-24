namespace Domain.Interfaces.Services.CoinService.Jobs;

public interface IDailyPriceRetentionJob
{
  Task RunOnceAsync(CancellationToken ct);
}