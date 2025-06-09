using Domain.Interfaces.Chat;
using Domain.Models.Dtos;

namespace Tests_back.Extensions;

public static class ChatExtention
{
    private static string fakeUser = GenerateFakeUser();

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

   
    private static string GenerateFakeUser()
    {
        return "test_user_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

}