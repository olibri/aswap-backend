using Domain.Enums;

namespace Domain.Models.Dtos;

public class UpdateOrderDto
{
    public ulong OrderId { get; set; }
    public decimal? MinFiatAmount { get; set; }
    public decimal? MaxFiatAmount { get; set; }

    public EscrowStatus? Status { get; set; }
    public string? BuyerFiat { get; set; }
    public string? SellerCrypto { get; set; }

    public decimal? FilledQuantity { get; set; }

}