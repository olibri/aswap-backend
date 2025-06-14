using Domain.Interfaces.Chat;
using Domain.Models.Dtos;

namespace Tests_back.Extensions;

public static class ChatExtention
{
    private static string fakeUser = AccountExtention.GenerateFakeUser();

    public static async Task CreateFakeMessageAsync(IChatDbCommand chatDbCommand,
     ulong dealId)
    {
        var messageDto = new MessageDto
        {
            DealId = dealId,
            AccountId = fakeUser,
            Content = "Test message content",
            CreatedAtUtc = DateTime.UtcNow
        };
        await chatDbCommand.CreateMessageAsync(messageDto);
    }
}