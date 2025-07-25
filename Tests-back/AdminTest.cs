using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;

namespace Tests_back;

public class AdminTest(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task Admin_Can_Ban_User()
  {
    PostgresDatabase.ResetState("bans");

    var wallet = WalletTestExtensions.CreateWallet().Address;
    var until = DateTime.UtcNow.AddDays(3);

    var id = await fixture.BanUserAsync(wallet, "spam", until);
    await fixture.AssertBannedUntilAsync(wallet, until);
  }
}