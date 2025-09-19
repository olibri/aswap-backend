using Domain.Interfaces.Services.Account;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;

namespace Tests_back;

public class ReferralServiceTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task GenerateReferralCode_NewUser_ReturnsUniqueCode()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var wallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var code = await referralService.GenerateReferralCodeAsync(wallet);

    // Assert
    code.ShouldNotBeNullOrEmpty();
    code.Length.ShouldBe(8);
    code.ShouldMatch("^[A-Z0-9]{8}$",
      "Code should contain only uppercase letters and numbers");
  }

  [Fact]
  public async Task GenerateReferralCode_ExistingUser_ReturnsSameCode()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var wallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var code1 = await referralService.GenerateReferralCodeAsync(wallet);
    var code2 = await referralService.GenerateReferralCodeAsync(wallet);

    // Assert
    code1.ShouldBe(code2, "Same user should get same referral code");
  }

  [Fact]
  public async Task GenerateReferralCode_MultipleUsers_ReturnsUniqueCodes()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referralService = fixture.GetService<IReferralService>();
    var codes = new List<string>();

    // Act
    for (int i = 0; i < 10; i++)
    {
      var wallet = ReferralExtensions.GenerateFakeWallet();
      var code = await referralService.GenerateReferralCodeAsync(wallet);
      codes.Add(code);
    }

    // Assert
    codes.Distinct().Count().ShouldBe(10, "All codes should be unique");
  }

  [Fact]
  public async Task GetReferralLink_ValidWallet_ReturnsCorrectLink()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var wallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var link = await referralService.GetReferralLinkAsync(wallet);

    // Assert
    link.ShouldNotBeNullOrEmpty();
    link.ShouldContain("/referral/");
    link.ShouldMatch(@"^https?://[^/]+/referral/[A-Z0-9]{8}$");
  }

  [Fact]
  public async Task ProcessReferral_ValidCode_ReturnsTrue()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referrer = await fixture.CreateFakeReferrerAsync();
    var newUserWallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var result = await referralService.ProcessReferralAsync(referrer.ReferralCode!, newUserWallet);

    // Assert
    result.ShouldBeTrue();
    await fixture.AssertReferralRelationshipAsync(referrer.WalletAddress, newUserWallet);
  }

  [Fact]
  public async Task ProcessReferral_InvalidCode_ReturnsFalse()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var invalidCode = "INVALID1";
    var newUserWallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var result = await referralService.ProcessReferralAsync(invalidCode, newUserWallet);

    // Assert
    result.ShouldBeFalse();
    await fixture.AssertNoReferralRelationshipAsync(newUserWallet);
  }

  [Fact]
  public async Task ProcessReferral_SelfReferral_ReturnsFalse()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referrer = await fixture.CreateFakeReferrerAsync();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var result = await referralService.ProcessReferralAsync(referrer.ReferralCode!, referrer.WalletAddress);

    // Assert
    result.ShouldBeFalse("Users should not be able to refer themselves");
  }

  [Fact]
  public async Task ProcessReferral_AlreadyReferred_ReturnsFalse()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referrer1 = await fixture.CreateFakeReferrerAsync();
    var referrer2 = await fixture.CreateFakeReferrerAsync();
    var newUserWallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var result1 = await referralService.ProcessReferralAsync(referrer1.ReferralCode!, newUserWallet);
    var result2 = await referralService.ProcessReferralAsync(referrer2.ReferralCode!, newUserWallet);

    // Assert
    result1.ShouldBeTrue();
    result2.ShouldBeFalse("User should not be able to be referred twice");
    await fixture.AssertReferralRelationshipAsync(referrer1.WalletAddress, newUserWallet);
  }

  [Fact]
  public async Task GetReferralStats_UserWithoutReferrals_ReturnsEmptyStats()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    PostgresDatabase.ResetState("referral_rewards");
    var wallet = ReferralExtensions.GenerateFakeWallet();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var stats = await referralService.GetReferralStatsAsync(wallet);

    // Assert
    stats.ShouldNotBeNull();
    stats.WalletAddress.ShouldBe(wallet);
    stats.ReferralCode.ShouldBeNull();
    stats.ReferralLink.ShouldBeNull();
    stats.TotalReferrals.ShouldBe(0);
    stats.TotalEarningsUsd.ShouldBe(0);
    stats.PendingRewards.ShouldBe(0);
    stats.PendingRewardsUsd.ShouldBe(0);
    stats.RecentRewards.ShouldBeEmpty();
  }

  [Fact]
  public async Task GetReferralStats_UserWithReferrals_ReturnsCorrectStats()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    PostgresDatabase.ResetState("referral_rewards");
    PostgresDatabase.ResetState("escrow_orders");

    var referrer = await fixture.CreateFakeReferrerAsync();
    var referralService = fixture.GetService<IReferralService>();
    
    // 🔥 ВИПРАВЛЕННЯ: Створюємо справжніх referee користувачів через ProcessReferralAsync
    var refereeWallets = new List<string>();
    for (int i = 0; i < 5; i++)
    {
      var refereeWallet = ReferralExtensions.GenerateFakeWallet();
      var processed = await referralService.ProcessReferralAsync(referrer.ReferralCode!, refereeWallet);
      processed.ShouldBeTrue($"Should successfully process referral for user {i}");
      refereeWallets.Add(refereeWallet);
    }
    
    // Створюємо нагороди для цих referee користувачів
    for (int i = 0; i < refereeWallets.Count; i++)
    {
      await fixture.CreateFakeReferralRewardAsync(
        referrer.WalletAddress, 
        refereeWallets[i], 
        20m + (i * 2.5m), 
        200m + (i * 25m));
    }
    
    // Оновлюємо earnings (ReferralCount вже оновлено через ProcessReferralAsync)
    await fixture.UpdateReferrerEarningsAsync(referrer.WalletAddress, 250m, 5);

    // Act
    var stats = await referralService.GetReferralStatsAsync(referrer.WalletAddress);

    // Assert
    stats.ShouldNotBeNull();
    stats.WalletAddress.ShouldBe(referrer.WalletAddress);
    stats.ReferralCode.ShouldBe(referrer.ReferralCode);
    stats.ReferralLink.ShouldNotBeNullOrEmpty();
    stats.TotalReferrals.ShouldBe(5);
    stats.PendingRewards.ShouldBeGreaterThan(0);
    stats.PendingRewardsUsd.ShouldBeGreaterThan(0);
    stats.RecentRewards.ShouldNotBeEmpty();
    stats.RecentRewards.Count.ShouldBeLessThanOrEqualTo(5);
  }

  [Fact]
  public async Task GetReferredUsers_WithPagination_ReturnsCorrectPage()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referrer = await fixture.CreateFakeReferrerAsync();
    var referralService = fixture.GetService<IReferralService>();

    // Створюємо 15 рефералів
    for (int i = 0; i < 15; i++)
    {
      await fixture.CreateFakeRefereeAsync(referrer.ReferralCode!);
    }

    // Act
    var page1 = await referralService.GetReferredUsersAsync(referrer.WalletAddress, page: 1, pageSize: 10);
    var page2 = await referralService.GetReferredUsersAsync(referrer.WalletAddress, page: 2, pageSize: 10);

    // Assert
    page1.Count.ShouldBe(10);
    page2.Count.ShouldBe(5);

    var allWallets = page1.Concat(page2).Select(u => u.WalletAddress).ToList();
    allWallets.Distinct().Count().ShouldBe(15, "All users should be unique");
  }

  [Fact]
  public async Task ValidateReferralCode_ValidCode_ReturnsTrue()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referrer = await fixture.CreateFakeReferrerAsync();
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var isValid = await referralService.ValidateReferralCodeAsync(referrer.ReferralCode!);

    // Assert
    isValid.ShouldBeTrue();
  }

  [Fact]
  public async Task ValidateReferralCode_InvalidCode_ReturnsFalse()
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var isValid = await referralService.ValidateReferralCodeAsync("INVALID1");

    // Assert
    isValid.ShouldBeFalse();
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("123")] // Too short
  [InlineData("ABCDEFGHI")] // Too long
  [InlineData("abcd1234")] // Lowercase
  [InlineData("ABC@1234")] // Special characters
  public async Task ValidateReferralCode_InvalidFormats_ReturnsFalse(string invalidCode)
  {
    // Arrange
    PostgresDatabase.ResetState("account");
    var referralService = fixture.GetService<IReferralService>();

    // Act
    var isValid = await referralService.ValidateReferralCodeAsync(invalidCode);

    // Assert
    isValid.ShouldBeFalse($"Code '{invalidCode}' should be invalid");
  }
}