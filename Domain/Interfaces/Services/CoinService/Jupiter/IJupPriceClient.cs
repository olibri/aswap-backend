using Domain.Models.Api.CoinPrice;

namespace Domain.Interfaces.Services.CoinService.Jupiter;

public interface IJupPriceClient
{
  Task<IReadOnlyDictionary<string, JupPriceItemDto>> GetUsdPricesAsync(
    IEnumerable<string> mints,
    CancellationToken ct);
}