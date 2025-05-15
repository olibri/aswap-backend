using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Enums;
using Domain.Models.Events;
using Domain.Models.Events.Helper;
using Riok.Mapperly.Abstractions;
using SimpleBase;
using System.Text;

namespace App.Mapper;

[Mapper]
public static partial class EscrowOrderMapper
{

    [MapProperty(
        nameof(OfferInitialized.OfferType),
        nameof(EscrowOrderEntity.OfferSide),
        Use = nameof(ToOrderSide)
    )]
    [MapProperty(nameof(OfferInitialized.Escrow), nameof(EscrowOrderEntity.EscrowPda),
        Use = nameof(ConvertHelper.ToBase58))]
    [MapProperty(nameof(OfferInitialized.Seller), nameof(EscrowOrderEntity.SellerCrypto),
        Use = nameof(ConvertHelper.ToBase58))]
    [MapProperty(nameof(OfferInitialized.TokenMint), nameof(EscrowOrderEntity.TokenMint),
        Use = nameof(ConvertHelper.ToBase58))]
    [MapProperty(nameof(OfferInitialized.FiatCode), nameof(EscrowOrderEntity.FiatCode),
        Use = nameof(ConvertHelper.Fiat))]
    [MapProperty(nameof(OfferInitialized.Ts), nameof(EscrowOrderEntity.CreatedAtUtc),
        Use = nameof(ConvertHelper.FromUnixSeconds))]
    public static partial EscrowOrderEntity ToEntity(OfferInitialized ev);

    private static void OnAfterToEntity(OfferInitialized src, EscrowOrderEntity dest)
    {
        dest.Id = Guid.NewGuid();
        dest.Status = EscrowStatus.PendingOnChain;
        dest.BuyerFiat = null;
    }

    //Trash because I use it in ConvertHelper
    private static string ToBase58(byte[] bytes) =>
        Base58.Bitcoin.Encode(bytes);

    private static OrderSide ToOrderSide(byte raw) => (OrderSide)raw;

    private static long ToUnixSeconds(DateTime dt) =>
        new DateTimeOffset(dt).ToUnixTimeSeconds();

    private static DateTime FromUnixSeconds(long ts) =>
        DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;


    private static string Fiat(byte[] code) =>
        Encoding.ASCII.GetString(code).TrimEnd('\0');

}