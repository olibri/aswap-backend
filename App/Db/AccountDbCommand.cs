using Domain.Interfaces.Database.Command;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Db;

public class AccountDbCommand(P2PDbContext dbContext) : IAccountDbCommand
{

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


    public async Task UpdateAccountInfoAsync(string token, long chatId, string userName)
    {
        var link = await dbContext.TelegramLinks
            .FirstOrDefaultAsync(l => l.Code == token);

        if (link is null || link.ExpiredAt < DateTime.UtcNow)
            return;

        var acc = await dbContext.Account
            .FirstOrDefaultAsync(a => a.WalletAddress == link.WalletAddress);

        if (acc is null)
            await UpsertAccountAsync(link.WalletAddress);

        acc.TelegramId = chatId.ToString();
        acc.Telegram = userName;
        acc.LastActiveTime = DateTime.UtcNow;

        dbContext.TelegramLinks.Remove(link);

        await dbContext.SaveChangesAsync();
    }

}