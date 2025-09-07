using Domain.Enums;
using Domain.Models.DB;

namespace Domain.Models.Dtos;

public sealed record ChildOrderDto(
  Guid Id,
  Guid ParentOrderId,
  ulong DealId,
  string OrderOwnerWallet,
  string ContraAgentWallet,
  EscrowStatus EscrowStatus,
  DateTime CreatedAtUtc,
  DateTime? ClosedAtUtc,
  int? FilledAmount,
  string? FillNonce,
  string? FillPda
)
{
  public static ChildOrderDto FromEntity(ChildOrderEntity e) => new(
    Id: e.Id,
    ParentOrderId: e.ParentOrderId,
    DealId: e.DealId,
    OrderOwnerWallet: e.OrderOwnerWallet,
    ContraAgentWallet: e.ContraAgentWallet,
    EscrowStatus: e.EscrowStatus,
    CreatedAtUtc: e.CreatedAtUtc,
    ClosedAtUtc: e.ClosedAtUtc,
    FilledAmount: e.FilledAmount,
    FillNonce: e.FillNonce,
    FillPda: e.FillPda
  );
}