using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using Domain.Models.Enums;

namespace Domain.Models.Dtos;

public class UpsertOrderDto : IValidatableObject
{
  public ulong OrderId { get; set; }
  [Required] public string FiatCode { get; set; } = null!;
  [Required] public string TokenMint { get; set; } = null!;
  [Required] public OrderSide OrderSide { get; set; }
  [Required] public PriceType PriceType { get; set; } = PriceType.Fixed;

  public EscrowStatus? Status { get; set; }
  public string? Buyer { get; set; }
  public string? Seller { get; set; }

  public string? EscrowPda { get; set; }

  [Range(0, double.MaxValue)] public decimal? Amount { get; set; }

  [Range(0, double.MaxValue)] public decimal? FilledQuantity { get; set; }

  [Range(0, double.MaxValue)] public decimal? MinFiatAmount { get; set; }

  [Range(0, double.MaxValue)] public decimal? MaxFiatAmount { get; set; }


  [Range(0, double.MaxValue)] public decimal? Price { get; set; }

  [Range(0, double.MaxValue)] public decimal? BasePrice { get; set; }
  public decimal? MarginPercent { get; set; }

  [MinLength(1)] public short[] PaymentMethodIds { get; set; } = Array.Empty<short>();

  [Range(1, 1440)] public int? PaymentWindowMinutes { get; set; }

  public OrderListingMode ListingMode { get; set; } = OrderListingMode.Online;

  public string[]? VisibleInCountries { get; set; }

  [Range(0, 36500)] public int? MinAccountAgeDays { get; set; }

  public string? Terms { get; set; } 
  public string[]? Tags { get; set; }
  public string? ReferralCode { get; set; } 
  public string? AutoReply { get; set; } 

  public bool? IsPratial { get; set; }
  public string? FillNonce { get; set; }
  public string? FillPda { get; set; }


  public bool? AdminCall { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext _)
  {
    // Мін/Макс
    if (MinFiatAmount is not null && MaxFiatAmount is not null && MinFiatAmount > MaxFiatAmount)
      yield return new ValidationResult("MinFiatAmount must be <= MaxFiatAmount",
        new[] { nameof(MinFiatAmount), nameof(MaxFiatAmount) });

    // Payment methods
    if (PaymentMethodIds is { Length: 0 })
      yield return new ValidationResult("At least one payment method required", new[] { nameof(PaymentMethodIds) });

    // Fixed vs Floating
    if (PriceType == PriceType.Fixed)
    {
      if (Price is null or <= 0)
        yield return new ValidationResult("Price is required for Fixed price type", new[] { nameof(Price) });

      if (BasePrice is not null || MarginPercent is not null)
        yield return new ValidationResult("BasePrice/MarginPercent must be null for Fixed price type",
          new[] { nameof(BasePrice), nameof(MarginPercent) });
    }
    else // Floating
    {
      if (BasePrice is null or <= 0)
        yield return new ValidationResult("BasePrice is required for Floating price type", new[] { nameof(BasePrice) });

      if (MarginPercent is null)
        yield return new ValidationResult("MarginPercent is required for Floating price type",
          new[] { nameof(MarginPercent) });
    }

    // Видимість країн — нормалізуємо формат (опційно можна робити в біндері)
    if (VisibleInCountries is { Length: > 0 })
      foreach (var c in VisibleInCountries)
        if (string.IsNullOrWhiteSpace(c) || c.Length is < 2 or > 3)
          yield return new ValidationResult($"Invalid country code: '{c}'", new[] { nameof(VisibleInCountries) });
  }
}