using Domain.Models.Enums;
using System.Text.Json.Serialization;

namespace Domain.Models.Dtos;

public class TgBotDto
{
    public ulong DealId { get; set; }
    public string? BuyerWallet { get; set; }
    public string? SellerWallet { get; set; }

    public string? OrderUrl { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationReceiver Receiver { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TgMessageType MessageType { get; set; }
}