using Domain.Interfaces.Chat;
using Shouldly;
using Tests_back.Extensions;

namespace Tests_back;

public class ChatTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    private readonly IChatDbCommand _dbChat = fixture.GetService<IChatDbCommand>();

    [Fact]
    public async Task GetMessages_ShouldReturnMessages()
    {
        var dealId = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await ChatExtention.CreateFakeMessageAsync(_dbChat, dealId);

        var messages = await _dbChat.GetMessagesAsync(dealId);
        messages.ShouldNotBeNull();
        messages.ShouldNotBeEmpty();
    }
}