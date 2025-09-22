using Domain.Enums;

namespace Domain.Models.Api.Order;

public sealed record ChildOrderUpsertDto(
  Guid ParentOrderId,
  ulong TicketId,
  string OrderOwnerWallet,
  string ContraAgentWallet,
  UniversalOrderStatus Status,
  decimal? Amount,
  string? TicketPda
);