using Domain.Interfaces.TelegramBot;
using Domain.Models.Enums;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class TgBotTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private readonly ITgBotHandler _dbChat = fixture.GetService<ITgBotHandler>();

    [Fact(Skip = "Non deterministic")]
    public async Task AdminCallTest()
    {
        var notification = TgBotExtention.CreateFakeAdminMessage(TgMessageType.AdminNotification);
        var message = await _dbChat.NotifyMessageAsync(notification);

        message.ShouldNotBeNull(); //TODO: think how to add addition assertions here
    }


    [Fact]
    public async Task UserCallTest()
    {
        var notification = TgBotExtention.CreateFakeAdminMessage(TgMessageType.OrderCreated);
        var message = await _dbChat.NotifyMessageAsync(notification);

        message.ShouldNotBeNull(); //TODO: think how to add addition assertions here
    }
}