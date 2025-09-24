using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Domain.Models.Dtos;

public class EscrowOrderDto
{
  public Guid Id { get; set; }
  public string? OrderPda { get; set; }
  public string? VaultPda { get; set; }
  public ulong OrderId { get; set; }

  public string? CreatorWallet { get; set; }
  public string? AcceptorWallet { get; set; }
  public string? TokenMint { get; set; }
  public string FiatCode { get; set; } = default!;

  public decimal? Amount { get; set; }
  public decimal? FilledQuantity { get; set; }
  public decimal Price { get; set; }

  [SwaggerSchema(Description =
    "UniversalOrderStatus: 0=Created, 1=Active, 2=SignedByOneParty, 3=BothSigned, 4=Completed, 5=Cancelled, 6=AdminResolving")]
  public UniversalOrderStatus Status { get; set; }

  public DateTime CreatedAtUtc { get; set; }
  public DateTime? ClosedAtUtc { get; set; }
  public DateTime? DealStartTime { get; set; }

  [SwaggerSchema(Description = "OrderSide: 0=Sell, 1=Buy")]
  public OrderSide OfferSide { get; set; }

  public bool IsPartial { get; set; }
  public decimal MinFiatAmount { get; set; }
  public decimal MaxFiatAmount { get; set; }
  public bool? AdminCall { get; set; }

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
  public DateTime? PaymentConfirmedAt { get; set; }
  public DateTime? CryptoReleasedAt { get; set; }
  public int? ReleaseTimeSeconds { get; set; }

  public List<PaymentMethodDto> PaymentMethods { get; set; } = new();
  public List<ChildOrderDto> Children { get; set; } = new();

  public static EscrowOrderDto FromEntity(EscrowOrderEntity e)
  {
    var dto = new EscrowOrderDto
    {
      Id = e.Id,
      OrderPda = e.OrderPda,
      VaultPda = e.VaultPda,
      OrderId = e.OrderId,

      CreatorWallet = e.CreatorWallet,
      AcceptorWallet = e.AcceptorWallet,
      TokenMint = e.TokenMint,
      FiatCode = e.FiatCode,

      Amount = e.Amount.HasValue ? e.Amount.Value / 1_000_000m : null,
      FilledQuantity = e.FilledQuantity / 1_000_000m,
      Price = e.Price / 100m,

      Status = e.Status,
      CreatedAtUtc = e.CreatedAtUtc,
      ClosedAtUtc = e.ClosedAtUtc,
      DealStartTime = e.DealStartTime,

      OfferSide = e.OfferSide,
      IsPartial = e.IsPartial,
      MinFiatAmount = e.MinFiatAmount,
      MaxFiatAmount = e.MaxFiatAmount,
      AdminCall = e.AdminCall,

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
      AutoReply = e.AutoReply,
      PaymentConfirmedAt = e.PaymentConfirmedAt,
      CryptoReleasedAt = e.CryptoReleasedAt,
      ReleaseTimeSeconds = e.ReleaseTimeSeconds
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

    if (e.ChildOrders is not null)
      foreach (var childOrder in e.ChildOrders)
        dto.Children.Add(ChildOrderDto.FromEntity(childOrder));

    return dto;
  }
}