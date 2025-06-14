using App.Mapper;
using Domain.Interfaces.Chat;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
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

    public async Task UpdateAccountInfoAsync(string token, long chatId, string userName)
    {
        var link = await dbContext.TelegramLinks
            .FirstOrDefaultAsync(l => l.Code == token);

        if (link is null || link.ExpiredAt < DateTime.UtcNow)
            return;

        var acc = await dbContext.Account
            .FirstOrDefaultAsync(a => a.WalletAddress == link.WalletAddress);

        if (acc is null)
            return;

        acc.TelegramId = chatId.ToString();
        acc.Telegram = userName;
        acc.LastActiveTime = DateTime.UtcNow;

        dbContext.TelegramLinks.Remove(link);

        await dbContext.SaveChangesAsync();
    }

    public async Task<string> GenerateCode(string wallet)
    {
        var code = CreateSecureToken(32);
        var entity = new TelegramLinkEntity
        {
            Code = code,
            WalletAddress = wallet,
            ExpiredAt = DateTime.UtcNow.AddDays(1)
        };
        dbContext.TelegramLinks.Add(entity);
        
        await dbContext.SaveChangesAsync();
     
        //return $"https://t.me/a_swap_bot?start={code}";
        return code;
    }

    public async Task UpsertAccountAsync(string accountWallet)
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

    private static string CreateSecureToken(int byteLength)
    {
        byte[] bytes = new byte[byteLength];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncoder.Encode(bytes);
    }
}