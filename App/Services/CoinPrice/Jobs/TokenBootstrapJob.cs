using Domain.Interfaces.Services.CoinService;
using Domain.Interfaces.Services.CoinService.Jobs;
using Domain.Interfaces.Services.CoinService.Jupiter;
using Domain.Interfaces.Services.CoinService.TokenRepo;

namespace App.Services.CoinPrice.Jobs;

public sealed class TokenBootstrapJob(ITokenRepository repo, IJupTokenClient client, IAppLockService @lock)
  : ITokenBootstrapJob
{
  public async Task RunOnceAsync(CancellationToken ct)
  {
    await using var handle = await @lock.TryAcquireAsync("token-bootstrap", ct);
    if (handle is null)
      return;

    if (await repo.AnyAsync(ct))
      return;

    var tokens = await client.GetVerifiedTokensAsync(ct);
    if (tokens.Count == 0)
      return;

    await repo.UpsertAsync(tokens, ct);
  }
}