using Domain.Models.Dtos;
using Domain.Models.Enums;

namespace Tests_back.Extensions;

public static class TgBotExtention
{
    public static TgBotDto CreateFakeAdminMessage(TgMessageType message)
    {
        return new TgBotDto
        {
            BuyerWallet = "FakeBuyerWallet",
            SellerWallet = "FakeSellerWallet",
            OrderId = 1234567890UL,
            OrderUrl = "https://fakeorder.url",
            MessageType = message,
        };
    }
}