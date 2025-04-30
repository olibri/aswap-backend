namespace Domain.Models.Events;

public class SellerSigned
{
    public string Escrow { get; set; }
    public ulong DealId { get; set; }
}