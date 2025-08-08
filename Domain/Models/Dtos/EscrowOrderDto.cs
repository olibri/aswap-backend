using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Domain.Models.Dtos;

public class EscrowOrderDto
{
    public Guid Id { get; set; }
    public string? EscrowPda { get; set; }
    public ulong DealId { get; set; }

    public string? SellerCrypto { get; set; }
    public string? BuyerFiat { get; set; }
    public string? TokenMint { get; set; }
    public string FiatCode { get; set; }

    public decimal? Amount { get; set; } 
    public decimal? FilledQuantity { get; set; }
    public decimal Price { get; set; }

    [SwaggerSchema(Description = "EscrowStatus: 0=PendingOnChain, 1=OnChain, 2=PartiallyOnChain, 3=Signed, 4=SignedByOneSide, Released=5, Cancelled=6, AdminResolving=7 ")]
    public EscrowStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    [SwaggerSchema(Description = "OrderSide: 0=Sell, 1=Buy")]
    public OrderSide OfferSide { get; set; }
    public decimal MinFiatAmount { get; set; }
    public decimal MaxFiatAmount { get; set; }

    public static EscrowOrderDto FromEntity(EscrowOrderEntity entity)
    {
        return new EscrowOrderDto
        {
            Id = entity.Id,
            EscrowPda = entity.EscrowPda,
            DealId = entity.DealId,
            SellerCrypto = entity.SellerCrypto,
            BuyerFiat = entity.BuyerFiat,
            TokenMint = entity.TokenMint,
            FiatCode = entity.FiatCode,
            Amount = (entity.Amount / 1000000m),
            Price = entity.Price / 100m,
            Status = entity.EscrowStatus,
            CreatedAtUtc = entity.CreatedAtUtc,
            ClosedAtUtc = entity.ClosedAtUtc,
            OfferSide = entity.OfferSide,
            MinFiatAmount = entity.MinFiatAmount,
            MaxFiatAmount = entity.MaxFiatAmount,
            FilledQuantity = entity.FilledQuantity
        };
    }
}