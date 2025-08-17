namespace Domain.Models.Api.CoinJelly;

public sealed record CoinJellyDto(
  Guid Id,
  string CompanyWalletAddress,
  string CryptoCurrency,
  string CryptoCurrencyChain);