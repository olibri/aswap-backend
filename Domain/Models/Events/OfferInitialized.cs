using Hexarc.Borsh.Serialization;
using System.Text;
using Domain.Models.Events.Helper;
using SimpleBase;
namespace Domain.Models.Events;



[BorshObject]
public class OfferInitialized
{
    [BorshPropertyOrder(0)]
    [BorshFixedArray(32)]
    public byte[] Escrow { get; set; } = new byte[32];

    [BorshPropertyOrder(1)]
    [BorshFixedArray(32)]
    public byte[] Seller { get; set; } = new byte[32];

    [BorshPropertyOrder(2)]
    [BorshFixedArray(32)]
    public byte[] TokenMint { get; set; } = new byte[32];

    [BorshPropertyOrder(3)]
    [BorshFixedArray(8)]                // ← [u8; 8] у Rust
    public byte[] FiatCode { get; set; } = new byte[8];

    [BorshPropertyOrder(4)] public ulong Amount { get; set; }
    [BorshPropertyOrder(5)] public ulong Price { get; set; }
    [BorshPropertyOrder(6)] public ulong DealId { get; set; }
    [BorshPropertyOrder(7)] public long Ts { get; set; }

    public override string ToString() =>                     // ← ось!
        $"OfferInitialized {{ Escrow={EventHelper.ToBase58(Escrow)}, " +
        $"Seller={EventHelper.ToBase58(Seller)}, Token={EventHelper.ToBase58(TokenMint)}, " +
        $"Fiat={EventHelper.Fiat(FiatCode)}, Amount={Amount}, Price={Price}, " +
        $"DealId={DealId}, Ts={Ts} }}";
}
