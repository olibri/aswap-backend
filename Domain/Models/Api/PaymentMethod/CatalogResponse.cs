namespace Domain.Models.Api.PaymentMethod;

public record CatalogResponse(
  PaymentResponse Payments,
  IReadOnlyList<CurrencyDto> Currencies);