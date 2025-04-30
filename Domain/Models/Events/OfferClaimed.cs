namespace Domain.Models.Events;

public class OfferClaimed
{
    public string Escrow { get; set; }
    public string Buyer { get; set; }
    public ulong DealId { get; set; }
}