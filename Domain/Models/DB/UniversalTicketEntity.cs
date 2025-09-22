using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("universal_tickets")]
public class UniversalTicketEntity
{
  [Key] [Column("id")] public Guid Id { get; set; } = Guid.NewGuid();

  [Column("parent_order_id")] public Guid ParentOrderId { get; set; }

  public EscrowOrderEntity ParentOrder { get; set; } = default!;

  [MaxLength(60)] [Column("ticket_pda")] public string? TicketPda { get; set; }

  [Column("ticket_id")] public ulong TicketId { get; set; }

  [Column("order_owner_wallet")] public string OrderOwnerWallet { get; set; } = default!;

  [Column("contra_agent_wallet")] public string ContraAgentWallet { get; set; } = default!;


  [Column("amount", TypeName = "numeric(20,0)")]
  public decimal? Amount { get; set; }

  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }
  [Column("closed_at_utc")] public DateTime? ClosedAtUtc { get; set; }

  [Column("updated_at")] public DateTime? UpdatedAt { get; set; }


  [Column("status")] public UniversalOrderStatus Status { get; set; }
}