using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Auth;
using Domain.Models.DB;
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

  public async Task<AccountEntity> CreateAccountWithReferralAsync(string walletAddress, string? referralCode = null, CancellationToken ct = default)
  {
    var existingAccount = await db.Account
      .FirstOrDefaultAsync(a => a.WalletAddress == walletAddress, ct);

    if (existingAccount != null)
    {
      return existingAccount; // Account already exists
    }

    var newAccount = new AccountEntity
    {
      WalletAddress = walletAddress,
      CreatedAtUtc = DateTime.UtcNow
    };

    // Process referral if provided
    if (!string.IsNullOrEmpty(referralCode))
    {
      var referrer = await db.Account
        .FirstOrDefaultAsync(a => a.ReferralCode == referralCode, ct);

      if (referrer != null && referrer.WalletAddress != walletAddress)
      {
        newAccount.ReferredBy = referrer.WalletAddress;

        // Update referrer's count
        referrer.ReferralCount++;
        db.Account.Update(referrer);
      }
    }

    db.Account.Add(newAccount);
    await db.SaveChangesAsync(ct);

    return newAccount;
  }
}