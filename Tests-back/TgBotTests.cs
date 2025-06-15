using Aswap_back.Controllers;
using Domain.Interfaces.Chat;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class TgBotTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private readonly ITgBotHandler _tgBot = fixture.GetService<ITgBotHandler>();
    private readonly IChatDbCommand _dbChat = fixture.GetService<IChatDbCommand>();
    private readonly IAccountDbCommand _dbAccount = fixture.GetService<IAccountDbCommand>();
    private readonly IAccountDbQueries _accountDbQueries = fixture.GetService<IAccountDbQueries>();
    private readonly TelegramHookController tgHookController = fixture.GetService<TelegramHookController>();
    private readonly PlatformController platformController = fixture.GetService<PlatformController>();

    [Fact(Skip = "Non deterministic")]
    public async Task AdminCallTest()
    {
        var notification = TgBotExtention.CreateFakeAdminMessage(TgMessageType.AdminNotification);
        var message = await _tgBot.NotifyMessageAsync(notification);

        message.ShouldNotBeNull(); //TODO: think how to add addition assertions here
    }


    [Fact]
    public async Task UserCallTest()
    {
        var notification = TgBotExtention.CreateFakeAdminMessage(TgMessageType.OrderCreated);
        var message = await _tgBot.NotifyMessageAsync(notification);

        message.ShouldNotBeNull(); //TODO: think how to add addition assertions here
    }

    [Fact]
    public async Task GeneratePrivateCode_Test()
    {
        var user = "user_0xZalupa123";
        var res = await platformController.PostCode(user);
        res.ShouldBeOfType<OkObjectResult>()
            .Value                   
            .ShouldSatisfyAllConditions(
                _ => _.ShouldNotBeNull(),
                _ => (_.GetType().GetProperty("code")!.GetValue(_) as string)
                    .ShouldNotBeNullOrWhiteSpace()
            );
    }

    [Fact(Skip = "Non deterministic")]
    public async Task TelegramHookTest()
    {
        var user = "user_0xZalupa123";
        var res = await _dbChat.GenerateCode(user);
        await AccountExtention.SaveFakeUserToDbAsync(user, _dbAccount);

        var updateDto = TgBotExtention.CreateFakeUpdateDto(res);
        var result = await tgHookController.Post(updateDto);
        result.ShouldNotBeNull();
        result.ShouldBeOfType<OkResult>();

        var account = await _accountDbQueries.GetAccountByWalletAsync(user);
        account.ShouldNotBeNull();
        account.WalletAddress.ShouldBe(user);
        account.TelegramId.ShouldNotBeNullOrEmpty();
        account.Telegram.ShouldNotBeNullOrEmpty();
    }

    [Fact(Skip = "Non deterministic")]
    public async Task NotifyUsersInTgTest()
    {
        var user = "user_0xZalupa12355";
        var res = await _dbChat.GenerateCode(user);
        await AccountExtention.SaveFakeUserToDbAsync(user, _dbAccount);
        await AccountExtention.UpdateFakeUserAsync(res, 5001098171, user, _dbAccount);

        var messageDto = new TgBotDto
        {
            BuyerWallet = user,
            DealId = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            MessageType = TgMessageType.OrderCreated,
            OrderUrl = "https://example.com/order/12345",
        };
        var messages = await _tgBot.NotifyMessageAsync(messageDto);

        messages.ShouldSatisfyAllConditions(
            () => messages.ShouldHaveSingleItem(),                
            () => messages[0].Chat.Id.ShouldBe(5001098171),
            () => messages[0].Text.ShouldContain(user)
        );
    }
}