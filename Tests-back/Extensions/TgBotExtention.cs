using Domain.Models.Dtos;
using Domain.Models.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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

    public static Update CreateFakeUpdateDto(string token)
    {
        var text = $"/start {token}";

        return new Update
        {
            Id = 2333,
            Message = new Message
            {
                Date = DateTime.UtcNow,
                Text = text,

                From = new User
                {
                    Id = 7360257367,
                    FirstName = "Fake",
                    IsBot = false,
                    Username = "fake_user"
                },

                Chat = new Chat
                {
                    Id = 7360257367,
                    Type = ChatType.Private,
                    Username = "fake_user"
                },

                Entities = new[]
                {
                    new MessageEntity
                    {
                        Offset = 0,
                        Length = 6,                
                        Type = MessageEntityType.BotCommand
                    }
                }
            },

            InlineQuery = null
        };
    }
}