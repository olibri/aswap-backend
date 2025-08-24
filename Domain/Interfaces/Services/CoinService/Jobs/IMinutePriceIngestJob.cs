namespace Domain.Interfaces.Services.CoinService.Jobs;

public interface IMinutePriceIngestJob
{
  // 2) Кожні 5 хв: зчитати всі мінти з БД, спланувати батчі, зробити запити, upsert
  Task RunOnceAsync(CancellationToken ct);
}