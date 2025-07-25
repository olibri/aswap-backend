﻿using Domain.Enums;
using Domain.Interfaces.Metrics;
using Domain.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.DB;

[Table("escrow_orders")]
public class EscrowOrderEntity: IHasDomainEvents
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [MaxLength(60)]
    [Column("escrow_pda")]
    public string? EscrowPda { get; set; }


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

    [Column("status")] public EscrowStatus Status { get; set; }

    [Column("created_at_utc")] public DateTime CreatedAtUtc { get; set; }


    [Column("closed_at_utc")] public DateTime? ClosedAtUtc { get; set; }

    [Column("offer_side")] public OrderSide OfferSide { get; set; }

    [Column("min_fiat_amount")] public decimal MinFiatAmount { get; set; }
    [Column("max_fiat_amount")] public decimal MaxFiatAmount { get; set; }

    [Column("filled_quantity")] public decimal FilledQuantity { get; set; }

    [Column("admin_call")] public bool? AdminCall { get; set; }

    [NotMapped] public List<DomainEvent> DomainEvents { get; } = new();

}
