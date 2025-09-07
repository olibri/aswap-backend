using Domain.Enums;

namespace Domain.Models.Api.Order;

public sealed record ChildOrderUpsertDto(
  Guid ParentOrderId,
  ulong DealId,
  string OrderOwnerWallet,
  string ContraAgentWallet,
  EscrowStatus EscrowStatus,
  int? FilledAmount,
  string? FillNonce,
  string? FillPda
);