namespace Domain.Interfaces.Services.CoinService;

public interface IPriceValidatorService
{
  Task<decimal?> GetUsdPriceAsync(string symbol, CancellationToken ct);
}