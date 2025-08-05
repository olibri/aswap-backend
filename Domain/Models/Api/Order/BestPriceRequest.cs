using Domain.Models.Enums;

namespace Domain.Models.Api.Order;

public readonly record struct BestPriceRequest(
  OrderSide Side,
  string FiatCode,
  string TokenMint,
  IReadOnlyList<short> MethodIds
);
