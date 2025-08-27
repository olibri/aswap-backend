using Domain.Models.Api.CoinPrice;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Services.CoinService;

public interface ICoinService
{
  Task<TokenDailyPriceResponse[]> GetPricesAsync(string coinX, string coinY);
  Task<TokenDto[]> GetCoinsAsync(CancellationToken ct);
}