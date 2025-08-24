namespace Domain.Interfaces.Services.CoinService;

public interface IAppLockService
{
  Task<IAsyncDisposable?> TryAcquireAsync(string lockName, CancellationToken ct);
}