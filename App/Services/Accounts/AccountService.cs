using Domain.Interfaces.Services;
using Domain.Models.Api.Auth;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Solnet.Wallet;

namespace App.Services.Accounts;

public sealed class AccountService(P2PDbContext db) : IAccountService
{
  public Task TouchAsync(string wallet, CancellationToken ct)
  {
    return db.Account.Where(a => a.WalletAddress == wallet)
      .ExecuteUpdateAsync(a =>
        a.SetProperty(x => x.LastActiveTime, DateTime.UtcNow), ct);
  }

  public async Task<DateTime?> GetBanUntilAsync(string wallet, CancellationToken ct)
  {
    var ban = await db.Bans
      .Where(b => b.Wallet == wallet)
      .OrderByDescending(b => b.BannedAt)
      .FirstOrDefaultAsync(ct);

    return ban?.Until;
  }

  public async Task<long> BanAsync(BanUserDto banUserDto, CancellationToken ct)
  {
    var ban = new BanEntity
    {
      Wallet = banUserDto.Wallet,
      Reason = banUserDto.Reason,
      BannedAt = DateTime.UtcNow,
      Until = banUserDto.Until
    };
    db.Bans.Add(ban);
    await db.SaveChangesAsync(ct);
    return ban.Id;
  }
}