using Domain.Interfaces.Hooks.Parsing;
using Domain.Models.Events.Helper;
using Hexarc.Borsh.Serialization;

namespace Domain.Models.Events;
public enum OfferKind : byte { Sell = 0, Buy = 1 }

[BorshObject]
public class OfferInitialized : IAnchorEvent
{
    [BorshPropertyOrder(0)]
    [BorshFixedArray(32)]
    public byte[] Escrow { get; set; } = new byte[32];

    [BorshPropertyOrder(1)]
    [BorshFixedArray(32)]
    public byte[] Seller { get; set; } = new byte[32];

    [BorshPropertyOrder(1)]
    [BorshFixedArray(32)]
    public byte[] Buyer { get; set; } = new byte[32];

    [BorshPropertyOrder(2)]
    [BorshFixedArray(32)]
    public byte[] TokenMint { get; set; } = new byte[32];

    [BorshPropertyOrder(3)]
    [BorshFixedArray(8)] // ← [u8; 8] у Rust
    public byte[] FiatCode { get; set; } = new byte[8];

    [BorshPropertyOrder(4)] public ulong Amount { get; set; }
    [BorshPropertyOrder(5)] public ulong Price { get; set; }
    [BorshPropertyOrder(6)] public ulong DealId { get; set; }
    [BorshPropertyOrder(7)] public long Ts { get; set; }

    [BorshPropertyOrder(8)] public byte OfferType { get; set; }

    public override string ToString() =>
        $"OfferInitialized {{ Escrow={ConvertHelper.ToBase58(Escrow)}, " +
        $"Seller={ConvertHelper.ToBase58(Seller)}, Token={ConvertHelper.ToBase58(TokenMint)}, " +
        $"Fiat={ConvertHelper.Fiat(FiatCode)}, Amount={Amount}, Price={Price}, " +
        $"DealId={DealId}, Ts={Ts}, OfferType={(OfferType == 0 ? "Sell" : "Buy")} }}";

}