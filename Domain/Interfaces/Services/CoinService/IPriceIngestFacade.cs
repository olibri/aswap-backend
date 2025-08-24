namespace Domain.Interfaces.Services.CoinService;

public interface IPriceIngestFacade
{
  Task BootstrapTokensAsync(CancellationToken ct);
  Task PollPricesAsync(CancellationToken ct);
  Task CleanupDailyAsync(CancellationToken ct);
}