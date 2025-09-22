using Domain.Interfaces.Hooks.Parsing;
using Domain.Models.Events.Helper;
using Hexarc.Borsh.Serialization;

namespace Domain.Models.Events;

public enum OfferKind : byte { Sell = 0, Buy = 1 }

[BorshObject]
public class UniversalOrderCreated : IAnchorEvent
{
  // 0
  [BorshPropertyOrder(0)]
  [BorshFixedArray(32)]
  public byte[] Order { get; set; } = new byte[32];

  // 1
  [BorshPropertyOrder(1)]
  [BorshFixedArray(32)]
  public byte[] Creator { get; set; } = new byte[32];

  // 2
  [BorshPropertyOrder(2)]
  [BorshFixedArray(32)]
  public byte[] CryptoMint { get; set; } = new byte[32];

  // 3 (Borsh bool)
  [BorshPropertyOrder(3)]
  public bool IsSellOrder { get; set; }

  // 4
  [BorshPropertyOrder(4)]
  public ulong CryptoAmount { get; set; }

  // 5
  [BorshPropertyOrder(5)]
  public ulong FiatAmount { get; set; }

  // 6
  [BorshPropertyOrder(6)]
  public ulong OrderId { get; set; }

  // 7
  [BorshPropertyOrder(7)]
  [BorshFixedArray(32)]
  public byte[] Vault { get; set; } = new byte[32];

  // 8 (i64 → long)
  [BorshPropertyOrder(8)]
  public long Timestamp { get; set; }

  // Зручний геттер для зворотної сумісності з OfferKind
  public OfferKind Kind => IsSellOrder ? OfferKind.Sell : OfferKind.Buy;

  public override string ToString() =>
    $"UniversalOrderCreated {{ " +
    $"Order={ConvertHelper.ToBase58(Order)}, " +
    $"Creator={ConvertHelper.ToBase58(Creator)}, " +
    $"Mint={ConvertHelper.ToBase58(CryptoMint)}, " +
    $"IsSellOrder={IsSellOrder}, " +
    $"CryptoAmount={CryptoAmount}, FiatAmount={FiatAmount}, " +
    $"OrderId={OrderId}, Vault={ConvertHelper.ToBase58(Vault)}, " +
    $"Ts={Timestamp} }}";
}