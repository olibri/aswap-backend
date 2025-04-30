namespace Domain.Models.Events;

public class BuyerSigned
{
    public string Escrow { get; set; }
    public ulong DealId { get; set; }
}