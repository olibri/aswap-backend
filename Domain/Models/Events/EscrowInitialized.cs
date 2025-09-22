using Domain.Interfaces.Hooks.Parsing;
using Domain.Models.Events.Helper;
using Hexarc.Borsh.Serialization;

namespace Domain.Models.Events;

[BorshObject]
public class EscrowInitialized : IAnchorEvent
{
    [BorshPropertyOrder(0)]
    [BorshFixedArray(32)]
    public byte[] Escrow { get; set; } = new byte[32];

    [BorshPropertyOrder(1)]
    [BorshFixedArray(32)]
    public byte[] Seller { get; set; } = new byte[32];

    [BorshPropertyOrder(2)]
    [BorshFixedArray(32)]
    public byte[] Buyer { get; set; } = new byte[32];

    [BorshPropertyOrder(3)]
    [BorshFixedArray(32)]
    public byte[] TokenMint { get; set; } = new byte[32];

    [BorshPropertyOrder(4)]
    [BorshFixedArray(8)]
    public byte[] FiatCode { get; set; } = new byte[8];

    [BorshPropertyOrder(5)] public ulong Amount { get; set; }
    [BorshPropertyOrder(6)] public ulong Price { get; set; }
    [BorshPropertyOrder(7)] public ulong DealId { get; set; }
    [BorshPropertyOrder(8)] public long Ts { get; set; }

    public override string ToString()
    {
        return $"EscrowInitialized {{ Escrow={ConvertHelper.ToBase58(Escrow)}, " +
               $"CreatorWallet={ConvertHelper.ToBase58(Seller)}, AcceptorWallet={ConvertHelper.ToBase58(Buyer)}, " +
               $"Token={ConvertHelper.ToBase58(TokenMint)}, Fiat={ConvertHelper.Fiat(FiatCode)}, " +
               $"Amount={Amount}, Price={Price}, DealId={DealId}, Ts={Ts} }}";
    }
}