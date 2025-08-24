using Domain.Interfaces.Services.CoinService;

namespace Tests_back.Extensions;

public sealed class TestFacade : IPriceIngestFacade
{
  public int PollCalls;
  public int CleanupCalls;

  public Task BootstrapTokensAsync(CancellationToken ct = default) => Task.CompletedTask;

  public Task PollPricesAsync(CancellationToken ct = default)
  {
    Interlocked.Increment(ref PollCalls);
    return Task.CompletedTask;
  }

  public Task CleanupDailyAsync(CancellationToken ct = default)
  {
    Interlocked.Increment(ref CleanupCalls);
    return Task.CompletedTask;
  }
}