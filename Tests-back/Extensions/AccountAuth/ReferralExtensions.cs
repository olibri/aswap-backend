using Domain.Interfaces.Services.Account;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Tests_back.Extensions.AccountAuth;

public static class ReferralExtensions
{
  private static readonly Random Rnd = new();

  public static string GenerateFakeWallet()
  {
    return "wallet_" + Guid.NewGuid().ToString("N")[..12];
  }

  public static async Task<AccountEntity> CreateFakeReferrerAsync(this TestFixture fixture, string? customWallet = null)
  {
    var wallet = customWallet ?? GenerateFakeWallet();
    var accountService = fixture.GetService<IAccountService>();
    var referralService = fixture.GetService<IReferralService>();

    _ = await accountService.CreateAccountWithReferralAsync(wallet);

    await referralService.GenerateReferralCodeAsync(wallet);

    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();
    var updatedAccount = await db.Account.FirstAsync(a => a.WalletAddress == wallet);

    return updatedAccount;
  }

  public static async Task<AccountEntity> CreateFakeRefereeAsync(this TestFixture fixture, string referralCode,
    string? customWallet = null)
  {
    var wallet = customWallet ?? GenerateFakeWallet();
    var accountService = fixture.GetService<IAccountService>();

    var account = await accountService.CreateAccountWithReferralAsync(wallet, referralCode);
    return account;
  }

  public static async Task<ReferralRewardEntity> CreateFakeReferralRewardAsync(
    this TestFixture fixture,
    string referrerWallet,
    string refereeWallet,
    decimal rewardUsd = 10.5m,
    decimal orderValueUsd = 100m,
    bool processed = false)
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();
    var accountService = fixture.GetService<IAccountService>();

    // 🔥 ВАЖЛИВО: Переконуємося, що обидва акаунти існують
    var referrerAccount = await db.Account.FirstOrDefaultAsync(a => a.WalletAddress == referrerWallet);
    if (referrerAccount == null)
    {
      await accountService.CreateAccountWithReferralAsync(referrerWallet);
    }

    var refereeAccount = await db.Account.FirstOrDefaultAsync(a => a.WalletAddress == refereeWallet);
    if (refereeAccount == null)
    {
      await accountService.CreateAccountWithReferralAsync(refereeWallet);
    }

    // Створюємо фейковий ордер
    var order = new EscrowOrderEntity
    {
      Id = Guid.NewGuid(),
      OrderId = (ulong)Rnd.NextInt64(1_000_000_000_000),
      OrderPda = $"OrderPda_{Guid.NewGuid():N}",
      VaultPda = $"VaultPda_{Guid.NewGuid():N}",
      CreatorWallet = referrerWallet,
      AcceptorWallet = refereeWallet,
      TokenMint = "USDC",
      FiatCode = "USD",
      Amount = (ulong)orderValueUsd,
      Price = 1,
      Status = Domain.Enums.UniversalOrderStatus.Completed,
      CreatedAtUtc = DateTime.UtcNow,
      OfferSide = Domain.Models.Enums.OrderSide.Sell,
      MinFiatAmount = 1,
      MaxFiatAmount = 1000,
      FilledQuantity = orderValueUsd,
      IsPartial = false
    };

    db.EscrowOrders.Add(order);
    await db.SaveChangesAsync();

    var reward = new ReferralRewardEntity
    {
      Id = Guid.NewGuid(),
      ReferrerWallet = referrerWallet,
      RefereeWallet = refereeWallet,
      OrderId = order.Id,
      RewardUsd = rewardUsd,
      RewardPercentage = 5.0m,
      OrderValueUsd = orderValueUsd,
      ProcessedAt = processed ? DateTime.UtcNow : null,
      CreatedAt = DateTime.UtcNow
    };

    db.ReferralRewards.Add(reward);
    await db.SaveChangesAsync();

    return reward;
  }

  public static async Task<List<ReferralRewardEntity>> CreateMultipleReferralRewardsAsync(
    this TestFixture fixture,
    string referrerWallet,
    int count,
    decimal baseRewardUsd = 10m,
    bool someProcessed = true)
  {
    var rewards = new List<ReferralRewardEntity>();
    var accountService = fixture.GetService<IAccountService>();

    // 🔥 ВАЖЛИВО: Переконуємося, що референт існує
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();
    var referrerExists = await db.Account.AnyAsync(a => a.WalletAddress == referrerWallet);
    if (!referrerExists)
    {
      await accountService.CreateAccountWithReferralAsync(referrerWallet);
    }

    for (var i = 0; i < count; i++)
    {
      var refereeWallet = GenerateFakeWallet();
      var isProcessed = someProcessed && i % 2 == 0; // Половина оброблених
      var rewardUsd = baseRewardUsd + i * 2.5m; // Різні суми

      // Створюємо referee акаунт
      await accountService.CreateAccountWithReferralAsync(refereeWallet);

      var reward = await fixture.CreateFakeReferralRewardAsync(
        referrerWallet,
        refereeWallet,
        rewardUsd,
        rewardUsd * 10,
        isProcessed);

      rewards.Add(reward);
    }

    return rewards;
  }

  public static async Task UpdateReferrerEarningsAsync(this TestFixture fixture, string wallet, decimal earnings,
    int referralCount = 0)
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var account = await db.Account.FirstAsync(a => a.WalletAddress == wallet);
    account.ReferralEarningsUsd = earnings;
    account.ReferralCount = referralCount;

    await db.SaveChangesAsync();
  }

  public static async Task<string> GetReferralCodeByWalletAsync(this TestFixture fixture, string wallet)
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var account = await db.Account.FirstOrDefaultAsync(a => a.WalletAddress == wallet);
    account.ShouldNotBeNull($"Account with wallet {wallet} should exist");
    account.ReferralCode.ShouldNotBeNullOrEmpty($"Account {wallet} should have referral code");

    return account.ReferralCode!;
  }

  public static async Task AssertReferralRelationshipAsync(this TestFixture fixture, string referrerWallet,
    string refereeWallet)
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var referee = await db.Account.FirstOrDefaultAsync(a => a.WalletAddress == refereeWallet);
    referee.ShouldNotBeNull($"Referee {refereeWallet} should exist");
    referee.ReferredBy.ShouldBe(referrerWallet, "Referee should be linked to referrer");

    var referrer = await db.Account.FirstOrDefaultAsync(a => a.WalletAddress == referrerWallet);
    referrer.ShouldNotBeNull($"Referrer {referrerWallet} should exist");
    referrer.ReferralCount.ShouldBeGreaterThan(0, "Referrer should have at least 1 referral");
  }

  public static async Task AssertNoReferralRelationshipAsync(this TestFixture fixture, string wallet)
  {
    await using var scope = fixture.Host.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var account = await db.Account.FirstOrDefaultAsync(a => a.WalletAddress == wallet);
    if (account != null) account.ReferredBy.ShouldBeNull("Account should not have a referrer");
  }

  // 🆕 Додатковий helper метод для створення референт-referee пари разом
  public static async Task<(AccountEntity referrer, AccountEntity referee)> CreateReferralPairAsync(
    this TestFixture fixture,
    string? referrerWallet = null,
    string? refereeWallet = null)
  {
    var referrer = await fixture.CreateFakeReferrerAsync(referrerWallet);
    var referee = await fixture.CreateFakeRefereeAsync(referrer.ReferralCode!, refereeWallet);

    return (referrer, referee);
  }

  // 🆕 Helper для створення готових рефереальних даних для тестів
  public static async Task<AccountEntity> CreateCompleteReferrerWithDataAsync(
    this TestFixture fixture,
    int refereeCount = 3,
    int rewardCount = 2)
  {
    var referrer = await fixture.CreateFakeReferrerAsync();

    // Створюємо кілька referee
    for (int i = 0; i < refereeCount; i++)
    {
      await fixture.CreateFakeRefereeAsync(referrer.ReferralCode!);
    }

    // Створюємо кілька нагород
    await fixture.CreateMultipleReferralRewardsAsync(referrer.WalletAddress, rewardCount);

    // Оновлюємо earnings
    await fixture.UpdateReferrerEarningsAsync(referrer.WalletAddress, rewardCount * 15m, refereeCount);

    return referrer;
  }
}