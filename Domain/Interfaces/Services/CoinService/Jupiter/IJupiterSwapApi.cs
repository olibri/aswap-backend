using Domain.Models.Api.CoinPrice;
using Domain.Models.Dtos.Jupiter;

namespace Domain.Interfaces.Services.CoinService.Jupiter;

public interface IJupiterSwapApi
{
  Task<QuoteResponseDto> GetQuoteAsync(QuoteRequest req, CancellationToken ct = default);

  Task<SwapResponseDto> CreateSwapAsync(string userPublicKey, QuoteResponseDto quote, SwapOptions? opts = null,
    CancellationToken ct = default);

}