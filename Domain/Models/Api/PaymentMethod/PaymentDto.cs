namespace Domain.Models.Api.PaymentMethod;

public sealed record PaymentDto(
  short Id,
  string Code,
  string Name,
  string Category
);