using Domain.Interfaces.Chat;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests_back.Extensions;

public static class ChatExtention
{
    private static string fakeUser = AccountExtention.GenerateFakeUser();

    public static async Task<Guid> CreateFakeRoomWithMessageAsync(this TestFixture fixture, ulong dealId)
    {
        // ? ????????? ??????? ??????? ? ??
        await fixture.CreateFakeRoomDirectlyAsync(dealId);

        // ????????? ???????????? ????? ??????
        var chatCommand = fixture.GetService<IChatDbCommand>();
        return await CreateFakeMessageAsync(chatCommand, dealId);
    }

    /// <summary>
    /// ??????? ??????? ??????? ? ?? (???????? ??????????? ?????????? ??????)
    /// </summary>
    public static async Task CreateFakeRoomDirectlyAsync(this TestFixture fixture, ulong dealId)
    {
        await using var scope = fixture.Host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

        // ??????????? ?? ??????? ??? ?????
        var existingRoom = await db.Rooms.FirstOrDefaultAsync(r => r.DealId == dealId);
        if (existingRoom != null) return;

        var room = new RoomEntity
        {
            DealId = dealId,
            CreatedAt = DateTime.UtcNow,
            LastMessageTime = DateTime.UtcNow
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// ??????? ?????? ???????????? (???? ??????? ??? ?????)
    /// </summary>
    public static async Task<Guid> CreateFakeMessageAsync(IChatDbCommand chatDbCommand, ulong dealId)
    {
        var messageDto = new MessageDto
        {
            DealId = dealId,
            AccountId = fakeUser,
            Content = "Test message content",
            CreatedAtUtc = DateTime.UtcNow
        };

        return await chatDbCommand.CreateMessageAsync(messageDto);
    }

    /// <summary>
    /// Extension ??? TestFixture
    /// </summary>
    public static async Task<Guid> CreateMessageForAttachmentTestAsync(this TestFixture fixture)
    {
        var dealId = (ulong)Random.Shared.NextInt64(1_000_000, 9_999_999);
        return await fixture.CreateFakeRoomWithMessageAsync(dealId);
    }
}