namespace Domain.Models.Api.PaymentMethod;

public record CatalogResponse(
  PaymentResponse Payments,
  CurrencyResponse Currencies);