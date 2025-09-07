using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Domain.Models.Dtos;

public sealed class PaymentCategoryDto
{
  public int Id { get; set; }
  public string Name { get; set; } = default!;
}

public sealed class PaymentMethodDto
{
  public int Id { get; set; }
  public string Code { get; set; } = default!;
  public string Name { get; set; } = default!;
  public PaymentCategoryDto? Category { get; set; }
}

public class EscrowOrderDto
{
  public Guid Id { get; set; }
  public string? EscrowPda { get; set; }
  public ulong DealId { get; set; }

  public string? SellerCrypto { get; set; }
  public string? BuyerFiat { get; set; }
  public string? TokenMint { get; set; }
  public string FiatCode { get; set; } = default!;

  public decimal? Amount { get; set; }
  public decimal? FilledQuantity { get; set; }
  public decimal Price { get; set; }

  [SwaggerSchema(Description =
    "EscrowStatus: 0=PendingOnChain, 1=OnChain, 2=PartiallyOnChain, 3=Signed, 4=SignedByOneSide, 5=Released, 6=Cancelled, 7=AdminResolving")]
  public EscrowStatus Status { get; set; }

  public DateTime CreatedAtUtc { get; set; }
  public DateTime? ClosedAtUtc { get; set; }

  [SwaggerSchema(Description = "OrderSide: 0=Sell, 1=Buy")]
  public OrderSide OfferSide { get; set; }

  public decimal MinFiatAmount { get; set; }
  public decimal MaxFiatAmount { get; set; }

  /* ───── new fields ───── */
  public PriceType PriceType { get; set; }
  public decimal? BasePrice { get; set; }
  public decimal? MarginPercent { get; set; }
  public int? PaymentWindowMinutes { get; set; }
  public OrderListingMode ListingMode { get; set; }
  public string[]? VisibleInCountries { get; set; }
  public int? MinAccountAgeDays { get; set; }
  public string? Terms { get; set; }
  public string[]? Tags { get; set; }
  public string? ReferralCode { get; set; }
  public string? AutoReply { get; set; }

  public List<PaymentMethodDto> PaymentMethods { get; set; } = new();


  public static EscrowOrderDto FromEntity(EscrowOrderEntity e)
  {
    var dto = new EscrowOrderDto
    {
      Id = e.Id,
      EscrowPda = e.EscrowPda,
      DealId = e.DealId,

      SellerCrypto = e.SellerCrypto,
      BuyerFiat = e.BuyerFiat,
      TokenMint = e.TokenMint,
      FiatCode = e.FiatCode,

      Amount = e.Amount.HasValue ? e.Amount.Value / 1_000_000m : null,
      FilledQuantity = e.FilledQuantity,
      Price = e.Price / 100m,

      Status = e.EscrowStatus,
      CreatedAtUtc = e.CreatedAtUtc,
      ClosedAtUtc = e.ClosedAtUtc,

      OfferSide = e.OfferSide,
      MinFiatAmount = e.MinFiatAmount,
      MaxFiatAmount = e.MaxFiatAmount,

      PriceType = e.PriceType,
      BasePrice = e.BasePrice,
      MarginPercent = e.MarginPercent,
      PaymentWindowMinutes = e.PaymentWindowMinutes,
      ListingMode = e.ListingMode,
      VisibleInCountries = e.VisibleInCountries,
      MinAccountAgeDays = e.MinAccountAgeDays,
      Terms = e.Terms,
      Tags = e.Tags,
      ReferralCode = e.ReferralCode,
      AutoReply = e.AutoReply
    };

    if (e.PaymentMethods is not null)
      foreach (var link in e.PaymentMethods)
      {
        var m = link.Method;
        if (m is null)
        {
          dto.PaymentMethods.Add(new PaymentMethodDto
          {
            Id = link.MethodId,
            Code = string.Empty,
            Name = string.Empty,
            Category = null
          });
          continue;
        }

        dto.PaymentMethods.Add(new PaymentMethodDto
        {
          Id = m.Id,
          Code = m.Code,
          Name = m.Name,
          Category = m.Category is null
            ? null
            : new PaymentCategoryDto
            {
              Id = m.Category.Id,
              Name = m.Category.Name
            }
        });
      }

    return dto;
  }
}