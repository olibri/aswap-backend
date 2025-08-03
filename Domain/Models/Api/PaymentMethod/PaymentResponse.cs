namespace Domain.Models.Api.PaymentMethod;

public sealed record PaymentResponse(
  IReadOnlyList<PaymentDto> Popular,
  IReadOnlyList<PaymentDto> All
);