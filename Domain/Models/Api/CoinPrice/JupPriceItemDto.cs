namespace Domain.Models.Api.CoinPrice;

public sealed record JupPriceItemDto(
  decimal UsdPrice,
  long BlockId,
  int Decimals,
  decimal PriceChange24h
);