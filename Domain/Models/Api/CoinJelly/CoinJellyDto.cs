namespace Domain.Models.Api.CoinJelly;

public sealed record CoinJellyDto(
  string CompanyWalletAddress,
  string CryptoCurrency,
  string CryptoCurrencyChain);