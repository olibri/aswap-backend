using Domain.Interfaces.Services.CoinService;
using Domain.Models.Dtos.BierdEye;

namespace App.Services.CoinPrice;

public class CoinService : ICoinService
{
  public Task<OhlcvCoinResponse> GetPricesAsync(string coinX, string coinY)
  {
    throw new NotImplementedException();
  }
}