using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jobs;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.Services.CoinService.TokenRepo;

namespace App.Services.CoinPrice;

public sealed class PriceIngestFacade(
  ITokenRepository tokens,
  IJupTokenClient tokenClient,
  ITokenBootstrapJob bootstrap,
  IMinutePriceIngestJob ingest,
  IDailyPriceRetentionJob retention)
  : IPriceIngestFacade
{
  private readonly IJupTokenClient _tokenClient = tokenClient;

  public async Task BootstrapTokensAsync(CancellationToken ct)
  {
    if (!await tokens.AnyAsync(ct))
      await bootstrap.RunOnceAsync(ct);
  }

  public async Task PollPricesAsync(CancellationToken ct)
  {
    if (!await tokens.AnyAsync(ct))
      await bootstrap.RunOnceAsync(ct);

    await ingest.RunOnceAsync(ct);
  }

  public Task CleanupDailyAsync(CancellationToken ct) =>
    retention.RunOnceAsync(ct);
}