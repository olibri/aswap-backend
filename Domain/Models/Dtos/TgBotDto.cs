using Domain.Models.Enums;

namespace Domain.Models.Dtos;

public class TgBotDto
{
    public ulong DealId { get; set; }
    public string? BuyerWallet { get; set; }
    public string? SellerWallet { get; set; }

    public string? OrderUrl { get; set; }

    public NotificationReceiver? Receiver { get; set; }
    public TgMessageType MessageType { get; set; }
}