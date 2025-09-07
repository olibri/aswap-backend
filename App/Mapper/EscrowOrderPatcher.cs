using Domain.Models.DB;
using Domain.Models.Dtos;

namespace App.Mapper;

public static class EscrowOrderPatcher
{
  public static void ApplyUpsert(
    EscrowOrderEntity entity,
    UpsertOrderDto dto,
    Func<EscrowOrderEntity, decimal, Domain.Enums.EscrowStatus>? moveToSignedStatus = null)
  {
    entity.DealId = dto.OrderId;
    entity.FiatCode = dto.FiatCode ?? entity.FiatCode;
    entity.TokenMint = dto.TokenMint ?? entity.TokenMint;

    if (dto.Status.HasValue) entity.EscrowStatus = dto.Status.Value;
    entity.BuyerFiat = dto.Buyer ?? entity.BuyerFiat;
    entity.SellerCrypto = dto.Seller ?? entity.SellerCrypto;
    entity.OfferSide = dto.OrderSide;

    if (dto.Amount.HasValue) entity.Amount = ToAtomic(dto.Amount.Value, 1_000_000m);
    if (dto.Price.HasValue) entity.Price = ToAtomic(dto.Price.Value, 100m);

    if (dto.FilledQuantity.HasValue)
    {
      if (moveToSignedStatus is not null)
        entity.EscrowStatus = moveToSignedStatus(entity, dto.FilledQuantity.Value);
      entity.FilledQuantity += dto.FilledQuantity.Value;
    }


    if (dto.MinFiatAmount.HasValue) entity.MinFiatAmount = dto.MinFiatAmount.Value;
    if (dto.MaxFiatAmount.HasValue) entity.MaxFiatAmount = dto.MaxFiatAmount.Value;

    entity.PriceType = dto.PriceType;
    entity.BasePrice = dto.BasePrice;
    entity.MarginPercent = dto.MarginPercent;
    entity.PaymentWindowMinutes = dto.PaymentWindowMinutes;
    entity.ListingMode = dto.ListingMode;
    entity.IsPartial = dto.IsPratial ?? entity.IsPartial;
    entity.EscrowPda = dto.EscrowPda ?? entity.EscrowPda;

    if (dto.VisibleInCountries is not null) entity.VisibleInCountries = dto.VisibleInCountries;
    if (dto.MinAccountAgeDays.HasValue) entity.MinAccountAgeDays = dto.MinAccountAgeDays;

    if (dto.Terms is not null) entity.Terms = dto.Terms;
    if (dto.Tags is not null) entity.Tags = dto.Tags;
    if (dto.ReferralCode is not null) entity.ReferralCode = dto.ReferralCode;
    if (dto.AutoReply is not null) entity.AutoReply = dto.AutoReply;

    if (dto.AdminCall.HasValue) entity.AdminCall = dto.AdminCall;

    if (dto.PaymentMethodIds is { Length: > 0 })
      SyncPaymentMethods(entity, dto.PaymentMethodIds);
  }

  private static void SyncPaymentMethods(EscrowOrderEntity entity, short[] methodIds)
  {
    var desired = methodIds.Distinct().ToHashSet();
    var current = entity.PaymentMethods.Select(pm => pm.MethodId).ToHashSet();

    foreach (var mid in desired.Except(current))
      entity.PaymentMethods.Add(new EscrowOrderPaymentMethodEntity { OrderId = entity.Id, MethodId = mid });

    var toRemove = entity.PaymentMethods
      .Where(pm => !desired.Contains(pm.MethodId)).ToList();

    foreach (var link in toRemove)
      entity.PaymentMethods.Remove(link);
  }

  private static ulong ToAtomic(decimal value, decimal multiplier)
  {
    var scaled = decimal.Round(value * multiplier, 0, MidpointRounding.AwayFromZero);
    if (scaled < 0 || scaled > (decimal)ulong.MaxValue)
      throw new OverflowException($"Value {value}×{multiplier} out of UInt64 range.");
    return (ulong)scaled;
  }
}