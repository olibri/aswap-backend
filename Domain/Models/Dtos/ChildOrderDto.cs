using Domain.Enums;
using Domain.Models.DB;

namespace Domain.Models.Dtos;

public sealed record ChildOrderDto(
  Guid Id,
  Guid ParentOrderId,
  ulong TicketId,
  string OrderOwnerWallet,
  string ContraAgentWallet,
  UniversalOrderStatus Status,
  DateTime CreatedAtUtc,
  DateTime? ClosedAtUtc,
  DateTime? UpdatedAt,
  decimal? Amount,
  string? TicketPda
)
{
  public static ChildOrderDto FromEntity(UniversalTicketEntity e) => new(
    Id: e.Id,
    ParentOrderId: e.ParentOrderId,
    TicketId: e.TicketId,
    OrderOwnerWallet: e.OrderOwnerWallet,
    ContraAgentWallet: e.ContraAgentWallet,
    Status: e.Status,
    CreatedAtUtc: e.CreatedAtUtc,
    ClosedAtUtc: e.ClosedAtUtc,
    UpdatedAt: e.UpdatedAt,
    Amount: e.Amount,
    TicketPda: e.TicketPda
  );
}