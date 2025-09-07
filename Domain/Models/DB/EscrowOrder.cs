using Domain.Enums;
using Domain.Interfaces.Metrics;
using Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("escrow_orders")]
public class EscrowOrderEntity : IHasDomainEvents
{
  [Key] [Column("id")] public Guid Id { get; set; }

  [MaxLength(60)] [Column("escrow_pda")] public string? EscrowPda { get; set; }


  [Column("deal_id")] public ulong DealId { get; set; }

  // on-chain state

  [Column("seller_crypto")] public string? SellerCrypto { get; set; }

  [Column("buyer_fiat")] public string? BuyerFiat { get; set; }

  [Column("token_mint")] public string? TokenMint { get; set; }

  [Column("fiat_code")] public string FiatCode { get; set; }

  [Column("amount", TypeName = "numeric(20,0)")]
  public ulong? Amount { get; set; }


  [Column("price", TypeName = "numeric(20,0)")]
  public ulong Price { get; set; }

  [Column("escrow_status")] public EscrowStatus EscrowStatus { get; set; }

  [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }


  [Column("closed_at_utc")] public DateTime? ClosedAtUtc { get; set; }

  [Column("offer_side")] public OrderSide OfferSide { get; set; }

  [Column("is_partial")] public bool IsPartial { get; set; } = false;

  [Column("min_fiat_amount")] public decimal MinFiatAmount { get; set; }
  [Column("max_fiat_amount")] public decimal MaxFiatAmount { get; set; }

  [Column("filled_quantity")] public decimal FilledQuantity { get; set; }

  [Column("admin_call")] public bool? AdminCall { get; set; }

  //new fields
  [Column("price_type")] public PriceType PriceType { get; set; } = PriceType.Fixed;

  [Column("base_price", TypeName = "numeric(38,18)")]
  public decimal? BasePrice { get; set; }

  [Column("margin_percent", TypeName = "numeric(38,18)")]
  public decimal? MarginPercent { get; set; }

  [Column("payment_window_minutes")] public int? PaymentWindowMinutes { get; set; }

  [Column("listing_mode")] public OrderListingMode ListingMode { get; set; } = OrderListingMode.Online;

  [Column("visible_countries", TypeName = "text[]")]
  public string[]? VisibleInCountries { get; set; }

  [Column("min_account_age_days")] public int? MinAccountAgeDays { get; set; }

  [Column("terms", TypeName = "text")] public string? Terms { get; set; }

  [Column("tags", TypeName = "text[]")] public string[]? Tags { get; set; }

  [Column("referral_code")] public string? ReferralCode { get; set; }

  [Column("auto_reply", TypeName = "text")]
  public string? AutoReply { get; set; }


  public ICollection<ChildOrderEntity> ChildOrders { get; set; } = new List<ChildOrderEntity>();
  public ICollection<EscrowOrderPaymentMethodEntity> PaymentMethods { get; set; } = [];

  [NotMapped] public List<DomainEvent> DomainEvents { get; } = new();
}