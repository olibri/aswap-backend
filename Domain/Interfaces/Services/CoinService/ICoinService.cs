using Domain.Models.Api.CoinPrice;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Services.CoinService;

public interface ICoinService
{
  Task<TokenDailyPriceResponse[]> GetPricesAsync(string coinX, string coinY);

  //get last actual price for coinX/coinY
  Task<(decimal, decimal)> GetLastPriceAsync(string coinX, string coinY);

  Task<TokenDto[]> GetCoinsAsync(CancellationToken ct);
}