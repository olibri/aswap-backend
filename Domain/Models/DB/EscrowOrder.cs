using Domain.Enums;

namespace Domain.Models.DB;

public class EscrowOrderEntity
{
    public Guid Id { get; set; }
    public string EscrowPda { get; set; }
    public string TxInitSig { get; set; }
    public ulong DealId { get; set; }

    // on-chain state
    public string Seller { get; set; }
    public string? Buyer { get; set; }
    public string TokenMint { get; set; }
    public string FiatCode { get; set; }
    public ulong Amount { get; set; }
    public ulong Price { get; set; }

    public EscrowStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}