using App.Mapper;
using Domain.Interfaces.Chat;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Chat;

public class ChatDbCommand(P2PDbContext dbContext): IChatDbCommand
{
    public async Task<Guid> CreateMessageAsync(MessageDto message)
    {
        try
        {
            await UpsertAccountAsync(message.AccountId);
            await UpsertRoomAsync(message);
            var res = await dbContext.Messages.AddAsync(ChatMapper.ToEntity(message));
            await dbContext.SaveChangesAsync();
            return res.Entity.Id;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<MessageDto[]> GetMessagesAsync(ulong roomId)
    {
        var entity = await dbContext.Messages
            .Where(m => m.RoomDealId == roomId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ToArrayAsync();

        return entity.Select(MessageDto.ToDto).ToArray();
    }

    private async Task UpsertAccountAsync(string accountWallet)
    {
        var existingAccount = await dbContext.Account
            .FirstOrDefaultAsync(a => a.WalletAddress == accountWallet);

        if (existingAccount == null)
        {
            await dbContext.Account.AddAsync(new AccountEntity
            {
                WalletAddress = accountWallet
            });
            await dbContext.SaveChangesAsync();
        }
        //TODO: create function to update account LastActiveTime
        else
        {
            existingAccount.LastActiveTime = DateTime.UtcNow;
            dbContext.Account.Update(existingAccount);
            await dbContext.SaveChangesAsync();
        }
    }
    private async Task UpsertRoomAsync(MessageDto message)
    {
        var existingRoom = await dbContext.Rooms
            .FirstOrDefaultAsync(r => r.DealId == message.DealId);
        if (existingRoom == null)
        {
            await CreateRoomAsync(message.DealId);
        }
        else
        {
            existingRoom.LastMessageTime = DateTime.UtcNow;
            dbContext.Rooms.Update(existingRoom);
            await dbContext.SaveChangesAsync();
        }
    }
    private async Task CreateRoomAsync(ulong dealId)
    {
        var res = await dbContext.Rooms.AddAsync(new RoomEntity
        {
            DealId = dealId,
        });

        await dbContext.SaveChangesAsync();
    }
}