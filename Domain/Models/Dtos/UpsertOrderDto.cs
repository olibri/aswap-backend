using Domain.Enums;
using Domain.Models.Enums;

namespace Domain.Models.Dtos;

public class UpsertOrderDto
{
    public ulong OrderId { get; set; }
    public decimal? MinFiatAmount { get; set; }
    public decimal? MaxFiatAmount { get; set; }

    public EscrowStatus? Status { get; set; }
    public string? Buyer { get; set; }
    public string? Seller { get; set; }

    public decimal? FilledQuantity { get; set; }

    public OrderSide? OrderType { get; set; }
    public string? FiatCode { get; set; }
    public string? TokenMint { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Price { get; set; }
    public short PaymentMethodId { get; init; }


    public bool? AdminCall { get; set; }
}