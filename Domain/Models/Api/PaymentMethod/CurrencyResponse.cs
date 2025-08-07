namespace Domain.Models.Api.PaymentMethod;

public sealed record CurrencyResponse(
  IReadOnlyList<CurrencyDto> Popular,
  IReadOnlyList<CurrencyDto> All
);