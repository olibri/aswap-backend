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
        nameof(UniversalOrderCreated.Order),
        nameof(EscrowOrderEntity.OrderPda),
        Use = nameof(ConvertHelper.ToBase58)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.Creator),
        nameof(EscrowOrderEntity.CreatorWallet),
        Use = nameof(ConvertHelper.ToBase58)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.CryptoMint),
        nameof(EscrowOrderEntity.TokenMint),
        Use = nameof(ConvertHelper.ToBase58)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.IsSellOrder),
        nameof(EscrowOrderEntity.OfferSide),
        Use = nameof(ToOrderSideFromBool)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.CryptoAmount),
        nameof(EscrowOrderEntity.Amount)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.FiatAmount),
        nameof(EscrowOrderEntity.FiatCode),
        Use = nameof(FiatAmountToCode)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.OrderId),
        nameof(EscrowOrderEntity.OrderId)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.Vault),
        nameof(EscrowOrderEntity.VaultPda),
        Use = nameof(ConvertHelper.ToBase58)
    )]
    [MapProperty(
        nameof(UniversalOrderCreated.Timestamp),
        nameof(EscrowOrderEntity.CreatedAtUtc),
        Use = nameof(ConvertHelper.FromUnixSeconds)
    )]
    public static partial EscrowOrderEntity ToEntity(UniversalOrderCreated ev);

    private static void OnAfterToEntity(UniversalOrderCreated src, EscrowOrderEntity dest)
    {
        dest.Id = Guid.NewGuid();
        dest.Status = UniversalOrderStatus.Created;
    }

    private static OrderSide ToOrderSideFromBool(bool isSellOrder) =>
        isSellOrder ? OrderSide.Sell : OrderSide.Buy;

    private static string FiatAmountToCode(ulong fiatAmount)
    {
        // Конвертація ulong в string для fiat code
        // Можливо потрібна інша логіка в залежності від того, як кодується fiat amount
        return fiatAmount.ToString();
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

    [MapProperty(nameof(UpsertOrderDto.OrderId), nameof(EscrowOrderEntity.OrderId))]
    [MapProperty(nameof(UpsertOrderDto.CreatorWallet), nameof(EscrowOrderEntity.CreatorWallet))]
    [MapProperty(nameof(UpsertOrderDto.AcceptorWallet), nameof(EscrowOrderEntity.AcceptorWallet))]
    [MapProperty(nameof(UpsertOrderDto.Status), nameof(EscrowOrderEntity.Status))]
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

        if (dto.DealStartTime.HasValue)
            entity.DealStartTime = dto.DealStartTime;
    }

    private static ulong ToAtomic(decimal value, decimal multiplier)
    {
        var scaled = decimal.Round(value * multiplier, 0, MidpointRounding.AwayFromZero);
        if (scaled < 0 || scaled > (decimal)ulong.MaxValue)
            throw new OverflowException($"Value {value}×{multiplier} виходить за межі UInt64.");
        return (ulong)scaled;
    }

}