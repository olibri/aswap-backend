namespace Domain.Models.Events;

public class FundsReleased
{
    public string Escrow { get; set; }
    public string Buyer { get; set; }
    public ulong Amount { get; set; }
    public ulong DealId { get; set; }
    public long Ts { get; set; }
}