using Domain.Models.Events.Helper;
using Hexarc.Borsh.Serialization;

namespace Domain.Models.Events;

[BorshObject]
public class OfferClaimed
{
    [BorshPropertyOrder(0)]
    [BorshFixedArray(32)]
    public byte[] Escrow { get; set; } = new byte[32];

    [BorshPropertyOrder(1)]
    [BorshFixedArray(32)]
    public byte[] Buyer { get; set; } = new byte[32];

    [BorshPropertyOrder(2)] public ulong DealId { get; set; }

    public override string ToString()
    {
        return $"OfferClaimed {{ Escrow={EventHelpers.ToBase58(Escrow)}, " +
               $"Buyer={EventHelpers.ToBase58(Buyer)}, DealId={DealId} }}";
    }
}