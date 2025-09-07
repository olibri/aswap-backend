using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("child_order")]
public class ChildOrderEntity
{
  [Key] [Column("id")] public Guid Id { get; set; }

  [Column("parent_order_id")] public Guid ParentOrderId { get; set; }

  public EscrowOrderEntity ParentOrder { get; set; } = default!;

  [Column("deal_id")] public ulong DealId { get; set; }

  [Column("order_owner_wallet")] public string OrderOwnerWallet { get; set; } = default!;
  [Column("contra_agent_wallet")] public string ContraAgentWallet { get; set; } = default!;

  [Column("escrow_status")] public EscrowStatus EscrowStatus { get; set; }

  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }
  [Column("closed_at_utc")] public DateTime? ClosedAtUtc { get; set; }


  [Column("filled_amount")] public int? FilledAmount { get; set; }
  [Column("fill_nonce")] public string? FillNonce { get; set; }
  [Column("fill_pda")] public string? FillPda { get; set; }
}