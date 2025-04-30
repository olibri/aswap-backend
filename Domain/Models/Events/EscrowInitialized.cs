namespace Domain.Models.Events;

public class EscrowInitialized
{
    public string Escrow { get; set; }
    public string Seller { get; set; }
    public string Buyer { get; set; }
    public string TokenMint { get; set; }
    public string FiatCode { get; set; }
    public ulong Amount { get; set; }
    public ulong Price { get; set; }
    public ulong DealId { get; set; }
    public long Ts { get; set; }
}