using Domain.Models.Api.CoinPrice;

namespace Domain.Interfaces.Services.CoinService.Jupiter;

public interface IJupTokenClient
{
  Task<IReadOnlyList<TokenDto>> GetVerifiedTokensAsync(CancellationToken ct);
}