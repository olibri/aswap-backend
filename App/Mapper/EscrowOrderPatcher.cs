using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Dtos;

namespace App.Mapper;

public static class EscrowOrderPatcher
{
  public static EscrowOrderPatchResult ApplyUpsert(
    EscrowOrderEntity entity,
    UpsertOrderDto dto)
  {
    var changed = new List<string>();
    decimal filledDelta = 0;

    var origStatus = entity.Status;
    var origAmount = entity.Amount;
    var origPrice = entity.Price;
    var origIsPartial = entity.IsPartial;

    if (entity.OrderId != dto.OrderId)
    {
      entity.OrderId = dto.OrderId;
      changed.Add(nameof(dto.OrderId));
    }

    // Required (but still confirm difference)
    if (dto.FiatCode is not null && entity.FiatCode != dto.FiatCode)
    {
      entity.FiatCode = dto.FiatCode;
      changed.Add(nameof(dto.FiatCode));
    }

    if (dto.TokenMint is not null && entity.TokenMint != dto.TokenMint)
    {
      entity.TokenMint = dto.TokenMint;
      changed.Add(nameof(dto.TokenMint));
    }

    if (dto.Status.HasValue && entity.Status != dto.Status.Value)
    {
      entity.Status = dto.Status.Value;
      changed.Add(nameof(dto.Status));
    }

    if (!string.IsNullOrWhiteSpace(dto.AcceptorWallet) && dto.AcceptorWallet != entity.AcceptorWallet)
    {
      entity.AcceptorWallet = dto.AcceptorWallet;
      changed.Add(nameof(dto.AcceptorWallet));
    }

    if (!string.IsNullOrWhiteSpace(dto.CreatorWallet) && dto.CreatorWallet != entity.CreatorWallet)
    {
      entity.CreatorWallet = dto.CreatorWallet;
      changed.Add(nameof(dto.CreatorWallet));
    }

    if (entity.OfferSide != dto.OrderSide)
    {
      entity.OfferSide = dto.OrderSide;
      changed.Add(nameof(dto.OrderSide));
    }

    if (dto.Amount.HasValue)
    {
      var newAtomic = ToAtomic(dto.Amount.Value, 1_000_000m);
      if (entity.Amount != newAtomic)
      {
        entity.Amount = newAtomic;
        changed.Add(nameof(dto.Amount));
      }
    }

    if (dto.Price.HasValue)
    {
      var newAtomicPrice = ToAtomic(dto.Price.Value, 100m);
      if (entity.Price != newAtomicPrice)
      {
        entity.Price = newAtomicPrice;
        changed.Add(nameof(dto.Price));
      }
    }

    if (dto.FilledQuantity.HasValue && dto.FilledQuantity.Value != 0)
    {
      entity.FilledQuantity += dto.FilledQuantity.Value;
      filledDelta = dto.FilledQuantity.Value;
      if (filledDelta != 0)
        changed.Add(nameof(dto.FilledQuantity));
    }

    if (dto.MinFiatAmount.HasValue && entity.MinFiatAmount != dto.MinFiatAmount.Value)
    {
      entity.MinFiatAmount = dto.MinFiatAmount.Value;
      changed.Add(nameof(dto.MinFiatAmount));
    }

    if (dto.MaxFiatAmount.HasValue && entity.MaxFiatAmount != dto.MaxFiatAmount.Value)
    {
      entity.MaxFiatAmount = dto.MaxFiatAmount.Value;
      changed.Add(nameof(dto.MaxFiatAmount));
    }

    if (entity.PriceType != dto.PriceType)
    {
      entity.PriceType = dto.PriceType;
      changed.Add(nameof(dto.PriceType));
    }

    if (dto.BasePrice.HasValue && entity.BasePrice != dto.BasePrice)
    {
      entity.BasePrice = dto.BasePrice;
      changed.Add(nameof(dto.BasePrice));
    }

    if (dto.MarginPercent.HasValue && entity.MarginPercent != dto.MarginPercent)
    {
      entity.MarginPercent = dto.MarginPercent;
      changed.Add(nameof(dto.MarginPercent));
    }

    if (dto.PaymentWindowMinutes.HasValue && entity.PaymentWindowMinutes != dto.PaymentWindowMinutes)
    {
      entity.PaymentWindowMinutes = dto.PaymentWindowMinutes;
      changed.Add(nameof(dto.PaymentWindowMinutes));
    }

    if (entity.ListingMode != dto.ListingMode)
    {
      entity.ListingMode = dto.ListingMode;
      changed.Add(nameof(dto.ListingMode));
    }

    if (dto.IsPartial.HasValue && entity.IsPartial != dto.IsPartial.Value)
    {
      entity.IsPartial = dto.IsPartial.Value;
      changed.Add(nameof(dto.IsPartial));
    }

    if (!string.IsNullOrWhiteSpace(dto.OrderPda) && dto.OrderPda != entity.OrderPda)
    {
      entity.OrderPda = dto.OrderPda;
      changed.Add(nameof(dto.OrderPda));
    }

    if (dto.DealStartTime.HasValue && entity.DealStartTime != dto.DealStartTime)
    {
      entity.DealStartTime = dto.DealStartTime;
      changed.Add(nameof(dto.DealStartTime));
    }

    if (dto.VisibleInCountries is not null &&
        (entity.VisibleInCountries is null ||
         !dto.VisibleInCountries.SequenceEqual(entity.VisibleInCountries)))
    {
      entity.VisibleInCountries = dto.VisibleInCountries;
      changed.Add(nameof(dto.VisibleInCountries));
    }

    if (dto.MinAccountAgeDays.HasValue && entity.MinAccountAgeDays != dto.MinAccountAgeDays)
    {
      entity.MinAccountAgeDays = dto.MinAccountAgeDays;
      changed.Add(nameof(dto.MinAccountAgeDays));
    }

    if (dto.Terms is not null && dto.Terms != entity.Terms)
    {
      entity.Terms = dto.Terms;
      changed.Add(nameof(dto.Terms));
    }

    if (dto.Tags is not null &&
        (entity.Tags is null || !dto.Tags.SequenceEqual(entity.Tags)))
    {
      entity.Tags = dto.Tags;
      changed.Add(nameof(dto.Tags));
    }

    if (dto.ReferralCode is not null && dto.ReferralCode != entity.ReferralCode)
    {
      entity.ReferralCode = dto.ReferralCode;
      changed.Add(nameof(dto.ReferralCode));
    }

    if (dto.AutoReply is not null && dto.AutoReply != entity.AutoReply)
    {
      entity.AutoReply = dto.AutoReply;
      changed.Add(nameof(dto.AutoReply));
    }

    if (dto.AdminCall.HasValue && entity.AdminCall != dto.AdminCall)
    {
      entity.AdminCall = dto.AdminCall;
      changed.Add(nameof(dto.AdminCall));
    }

    bool paymentMethodsChanged = false;
    if (dto.PaymentMethodIds is { Length: > 0 })
      paymentMethodsChanged = SyncPaymentMethods(entity, dto.PaymentMethodIds);

    if (paymentMethodsChanged)
      changed.Add("PaymentMethods");

    var result = new EscrowOrderPatchResult(
      AnyChanges: changed.Count > 0,
      ChangedFields: changed,
      StatusChanged: origStatus != entity.Status,
      FilledQuantityDelta: filledDelta,
      NewFilledQuantity: entity.FilledQuantity,
      AmountChanged: entity.Amount != origAmount ? entity.Amount : null,
      PriceChanged: entity.Price != origPrice ? entity.Price : null,
      IsPartialChanged: origIsPartial != entity.IsPartial,
      PaymentMethodsChanged: paymentMethodsChanged
    );

    return result;
  }

  private static bool SyncPaymentMethods(EscrowOrderEntity entity, short[] methodIds)
  {
    var desired = methodIds.Distinct().ToHashSet();
    var current = entity.PaymentMethods.Select(pm => pm.MethodId).ToHashSet();

    var added = false;
    foreach (var mid in desired.Except(current))
    {
      entity.PaymentMethods.Add(new EscrowOrderPaymentMethodEntity { OrderId = entity.Id, MethodId = mid });
      added = true;
    }

    var toRemove = entity.PaymentMethods
      .Where(pm => !desired.Contains(pm.MethodId))
      .ToList();

    var removed = toRemove.Count > 0;
    foreach (var link in toRemove)
      entity.PaymentMethods.Remove(link);

    return added || removed;
  }

  private static ulong ToAtomic(decimal value, decimal multiplier)
  {
    var scaled = decimal.Round(value * multiplier, 0, MidpointRounding.AwayFromZero);
    if (scaled < 0 || scaled > (decimal)ulong.MaxValue)
      throw new OverflowException($"Value {value}×{multiplier} out of UInt64 range.");
    return (ulong)scaled;
  }
}

public sealed record EscrowOrderPatchResult(
  bool AnyChanges,
  IReadOnlyCollection<string> ChangedFields,
  bool StatusChanged,
  decimal FilledQuantityDelta,
  decimal NewFilledQuantity,
  ulong? AmountChanged,
  ulong? PriceChanged,
  bool IsPartialChanged,
  bool PaymentMethodsChanged
);