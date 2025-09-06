using Domain.Enums;
using Domain.Models.Enums;

namespace Domain.Models.Api.Order;

public sealed record UserOffersQuery(
  EscrowStatus? Status,
  string? TokenMint,
  string? FiatCode,
  OrderSide? Side,
  DateTime? From,
  DateTime? To,
  SortDir Dir = SortDir.Desc,
  int Page = 1,
  int Size = 20
);