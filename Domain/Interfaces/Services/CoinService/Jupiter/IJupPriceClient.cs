using Domain.Models.Api.CoinPrice;
using Microsoft.AspNetCore.Builder.Extensions;

namespace Domain.Interfaces.Services.CoinService.Jupiter;

public interface IJupPriceClient
{
  Task<IReadOnlyDictionary<string, JupPriceItemDto>> GetUsdPricesAsync(
    IEnumerable<string> mints,
    CancellationToken ct);
}