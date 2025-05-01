using Domain.Models.Events.Helper;
using Hexarc.Borsh.Serialization;

namespace Domain.Models.Events;

[BorshObject]
public class BuyerSigned
{
    [BorshPropertyOrder(0)]
    [BorshFixedArray(32)]
    public byte[] Escrow { get; set; } = new byte[32];

    [BorshPropertyOrder(1)] public ulong DealId { get; set; }

    public override string ToString()
    {
        return $"BuyerSigned {{ Escrow={EventHelpers.ToBase58(Escrow)}, DealId={DealId} }}";
    }
}