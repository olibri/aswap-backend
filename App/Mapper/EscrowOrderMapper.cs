using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Dtos;
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
        dest.EscrowStatus = EscrowStatus.PendingOnChain;
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


    [MapProperty(nameof(UpsertOrderDto.OrderId), nameof(EscrowOrderEntity.DealId))]
    [MapProperty(nameof(UpsertOrderDto.Seller), nameof(EscrowOrderEntity.SellerCrypto))]
    [MapProperty(nameof(UpsertOrderDto.Buyer), nameof(EscrowOrderEntity.BuyerFiat))]
    [MapProperty(nameof(UpsertOrderDto.Status), nameof(EscrowOrderEntity.EscrowStatus))]
    [MapProperty(nameof(UpsertOrderDto.OrderSide), nameof(EscrowOrderEntity.OfferSide))]
    [MapProperty(nameof(UpsertOrderDto.MinFiatAmount), nameof(EscrowOrderEntity.MinFiatAmount))]
    [MapProperty(nameof(UpsertOrderDto.MaxFiatAmount), nameof(EscrowOrderEntity.MaxFiatAmount))]
    [MapProperty(nameof(UpsertOrderDto.FiatCode), nameof(EscrowOrderEntity.FiatCode))]
    [MapProperty(nameof(UpsertOrderDto.TokenMint), nameof(EscrowOrderEntity.TokenMint))]
    [MapProperty(nameof(UpsertOrderDto.Amount), nameof(EscrowOrderEntity.Amount))]
    [MapProperty(nameof(UpsertOrderDto.Price), nameof(EscrowOrderEntity.Price))]

    [MapProperty(nameof(UpsertOrderDto.PriceType), nameof(EscrowOrderEntity.PriceType))]
    [MapProperty(nameof(UpsertOrderDto.BasePrice), nameof(EscrowOrderEntity.BasePrice))]
    [MapProperty(nameof(UpsertOrderDto.MarginPercent), nameof(EscrowOrderEntity.MarginPercent))]
    [MapProperty(nameof(UpsertOrderDto.PaymentWindowMinutes), nameof(EscrowOrderEntity.PaymentWindowMinutes))]
    [MapProperty(nameof(UpsertOrderDto.ListingMode), nameof(EscrowOrderEntity.ListingMode))]
    [MapProperty(nameof(UpsertOrderDto.VisibleInCountries), nameof(EscrowOrderEntity.VisibleInCountries))]
    [MapProperty(nameof(UpsertOrderDto.MinAccountAgeDays), nameof(EscrowOrderEntity.MinAccountAgeDays))]
    [MapProperty(nameof(UpsertOrderDto.Terms), nameof(EscrowOrderEntity.Terms))]
    [MapProperty(nameof(UpsertOrderDto.Tags), nameof(EscrowOrderEntity.Tags))]
    [MapProperty(nameof(UpsertOrderDto.ReferralCode), nameof(EscrowOrderEntity.ReferralCode))]
    [MapProperty(nameof(UpsertOrderDto.AutoReply), nameof(EscrowOrderEntity.AutoReply))]
    [MapProperty(nameof(UpsertOrderDto.FilledQuantity), nameof(EscrowOrderEntity.FilledQuantity))]
    [MapProperty(nameof(UpsertOrderDto.AdminCall), nameof(EscrowOrderEntity.AdminCall))]
    [MapProperty(nameof(UpsertOrderDto.DealStartTime), nameof(EscrowOrderEntity.DealStartTime))]
    public static partial EscrowOrderEntity ToEntity(UpsertOrderDto dto);

    private static void OnAfterToEntity(UpsertOrderDto dto, EscrowOrderEntity entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAtUtc = DateTime.UtcNow;
        }

        if (dto.Amount is not null)
            entity.Amount = ToAtomic(dto.Amount.Value, 1_000_000m);

        entity.Price = dto.Price is not null ? ToAtomic(dto.Price.Value, 100m) : 0UL;
    }

    private static ulong ToAtomic(decimal value, decimal multiplier)
    {
        var scaled = decimal.Round(value * multiplier, 0, MidpointRounding.AwayFromZero);
        if (scaled < 0 || scaled > (decimal)ulong.MaxValue)
            throw new OverflowException($"Value {value}×{multiplier} виходить за межі UInt64.");
        return (ulong)scaled;
    }

}