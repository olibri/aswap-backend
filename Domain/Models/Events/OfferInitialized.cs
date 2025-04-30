using Hexarc.Borsh.Serialization;
using System.Buffers.Text;
using System.Text;
using SimpleBase;
using Solnet.Programs.Utilities;
namespace Domain.Models.Events;



[BorshObject]
public class OfferInitialized
{
    static string ToBase58(byte[] bytes)
        => Base58.Bitcoin.Encode(bytes);
    static string Fiat(byte[] code)     
        => Encoding.ASCII.GetString(code).TrimEnd('\0');

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
        $"OfferInitialized {{ Escrow={ToBase58(Escrow)}, " +
        $"Seller={ToBase58(Seller)}, Token={ToBase58(TokenMint)}, " +
        $"Fiat={Fiat(FiatCode)}, Amount={Amount}, Price={Price}, " +
        $"DealId={DealId}, Ts={Ts} }}";


}
