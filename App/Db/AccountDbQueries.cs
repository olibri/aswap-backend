using Domain.Interfaces.Database.Queries;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Db;

public class AccountDbQueries(P2PDbContext dbContext) : IAccountDbQueries
{
    public async Task<AccountDto?> GetAccountByWalletAsync(string wallet)
    {
        var account = await dbContext.Account.FirstOrDefaultAsync(a => a.WalletAddress == wallet);
        return account != null ? AccountDto.ToDto(account) : null;
    }
}