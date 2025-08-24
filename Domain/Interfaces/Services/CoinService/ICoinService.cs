using Domain.Models.Dtos.BierdEye;

namespace Domain.Interfaces.Services.CoinService;

public interface ICoinService
{
  Task<OhlcvCoinResponse> GetPricesAsync(string coinX, string coinY);
}