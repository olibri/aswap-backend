namespace Domain.Models.Api.PaymentMethod;

public sealed record CurrencyDto(
  short Id,
  string Code,
  string Name);