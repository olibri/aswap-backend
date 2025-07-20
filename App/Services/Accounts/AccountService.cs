using Domain.Interfaces.Services;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Accounts;

public sealed class AccountService(P2PDbContext db) : IAccountService
{
  public Task TouchAsync(string wallet, CancellationToken ct) =>
    db.Account.Where(a => a.WalletAddress == wallet)
      .ExecuteUpdateAsync(a =>
        a.SetProperty(x => x.LastActiveTime, DateTime.UtcNow), ct);
}