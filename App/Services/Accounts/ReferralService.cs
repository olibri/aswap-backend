using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Referral;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace App.Services.Accounts;

public sealed class ReferralService(P2PDbContext db, IConfiguration configuration)
  : IReferralService
{
  private const int ReferralCodeLength = 8;
  private const string ReferralCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

  public async Task<string> GenerateReferralCodeAsync(string walletAddress, CancellationToken ct = default)
  {
    // Check if user already has a referral code
    var existingAccount = await db.Account
      .FirstOrDefaultAsync(a => a.WalletAddress == walletAddress, ct);

    if (existingAccount?.ReferralCode != null) return existingAccount.ReferralCode;

    // Generate unique referral code
    string referralCode;
    bool isUnique;
    do
    {
      referralCode = GenerateRandomCode();
      isUnique = !await db.Account
        .AnyAsync(a => a.ReferralCode == referralCode, ct);
    } while (!isUnique);

    // Update or create account with referral code
    if (existingAccount != null)
    {
      existingAccount.ReferralCode = referralCode;
      db.Account.Update(existingAccount);
    }
    else
    {
      var newAccount = new AccountEntity
      {
        WalletAddress = walletAddress,
        ReferralCode = referralCode,
        CreatedAtUtc = DateTime.UtcNow
      };
      db.Account.Add(newAccount);
    }

    await db.SaveChangesAsync(ct);
    return referralCode;
  }

  public async Task<string> GetReferralLinkAsync(string walletAddress, CancellationToken ct = default)
  {
    var referralCode = await GenerateReferralCodeAsync(walletAddress, ct);
    var baseUrl = configuration["App:BaseUrl"] ?? "https://a-swap.xyz";
    return $"{baseUrl}/referral/{referralCode}";
  }

  public async Task<bool> ProcessReferralAsync(string referralCode, string newUserWallet,
    CancellationToken ct = default)
  {
    // Find the referrer by referral code
    var referrer = await db.Account
      .FirstOrDefaultAsync(a => a.ReferralCode == referralCode, ct);

    if (referrer == null) return false; // Invalid referral code

    // Check if new user already exists and is not already referred
    var existingUser = await db.Account
      .FirstOrDefaultAsync(a => a.WalletAddress == newUserWallet, ct);

    if (existingUser?.ReferredBy != null) return false; // User already has a referrer

    // Prevent self-referral
    if (referrer.WalletAddress == newUserWallet) return false;

    // Create or update the new user account
    if (existingUser != null)
    {
      existingUser.ReferredBy = referrer.WalletAddress;
      db.Account.Update(existingUser);
    }
    else
    {
      var newAccount = new AccountEntity
      {
        WalletAddress = newUserWallet,
        ReferredBy = referrer.WalletAddress,
        CreatedAtUtc = DateTime.UtcNow
      };
      db.Account.Add(newAccount);
    }

    // Update referrer's referral count
    referrer.ReferralCount++;
    db.Account.Update(referrer);

    await db.SaveChangesAsync(ct);
    return true;
  }

  public async Task<ReferralStatsDto> GetReferralStatsAsync(string walletAddress, CancellationToken ct = default)
  {
    var account = await db.Account
      .FirstOrDefaultAsync(a => a.WalletAddress == walletAddress, ct);

    if (account == null) return new ReferralStatsDto { WalletAddress = walletAddress };

    // Get recent rewards
    var recentRewards = await db.ReferralRewards
      .Where(r => r.ReferrerWallet == walletAddress)
      .OrderByDescending(r => r.CreatedAt)
      .Take(10)
      .Select(r => new ReferralRewardDto
      {
        Id = r.Id,
        RefereeWallet = r.RefereeWallet,
        OrderId = r.OrderId,
        RewardUsd = r.RewardUsd,
        RewardPercentage = r.RewardPercentage,
        OrderValueUsd = r.OrderValueUsd,
        CreatedAt = r.CreatedAt,
        ProcessedAt = r.ProcessedAt
      })
      .ToListAsync(ct);

    // Calculate pending rewards
    var pendingRewards = await db.ReferralRewards
      .Where(r => r.ReferrerWallet == walletAddress && r.ProcessedAt == null)
      .GroupBy(r => 1)
      .Select(g => new
      {
        Count = g.Count(),
        TotalUsd = g.Sum(r => r.RewardUsd)
      })
      .FirstOrDefaultAsync(ct);

    var referralLink = account.ReferralCode != null
      ? await GetReferralLinkAsync(walletAddress, ct)
      : null;

    return new ReferralStatsDto
    {
      WalletAddress = walletAddress,
      ReferralCode = account.ReferralCode,
      ReferralLink = referralLink,
      TotalReferrals = account.ReferralCount,
      TotalEarningsUsd = account.ReferralEarningsUsd,
      PendingRewards = pendingRewards?.Count ?? 0,
      PendingRewardsUsd = pendingRewards?.TotalUsd ?? 0,
      RecentRewards = recentRewards
    };
  }

  public async Task<List<ReferredUserDto>> GetReferredUsersAsync(string walletAddress, int page = 1, int pageSize = 20,
    CancellationToken ct = default)
  {
    var skip = (page - 1) * pageSize;

    var referredUsers = await db.Account
      .Where(a => a.ReferredBy == walletAddress)
      .OrderByDescending(a => a.CreatedAtUtc)
      .Skip(skip)
      .Take(pageSize)
      .Select(a => new ReferredUserDto
      {
        WalletAddress = a.WalletAddress,
        JoinedAt = a.CreatedAtUtc,
        OrdersCount = a.OrdersCount ?? 0,
        // Calculate total volume and earnings in a separate query if needed
        TotalVolumeUsd = 0, // TODO: Calculate from orders
        EarnedFromThisUserUsd = db.ReferralRewards
          .Where(r => r.ReferrerWallet == walletAddress && r.RefereeWallet == a.WalletAddress)
          .Sum(r => r.RewardUsd)
      })
      .ToListAsync(ct);

    return referredUsers;
  }

  public async Task<bool> ValidateReferralCodeAsync(string referralCode, CancellationToken ct = default)
  {
    return await db.Account
      .AnyAsync(a => a.ReferralCode == referralCode, ct);
  }

  private static string GenerateRandomCode()
  {
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[ReferralCodeLength];
    rng.GetBytes(bytes);

    var result = new StringBuilder(ReferralCodeLength);
    for (var i = 0; i < ReferralCodeLength; i++) result.Append(ReferralCodeChars[bytes[i] % ReferralCodeChars.Length]);

    return result.ToString();
  }
}