namespace Domain.Models.Api.CoinPrice;

public sealed record TokenDto(
  string Mint,
  string? Symbol,
  string? Name,
  int? Decimals,
  bool? IsVerified
);