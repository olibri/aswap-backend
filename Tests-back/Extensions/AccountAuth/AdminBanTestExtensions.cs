using Aswap_back.Controllers;
using Domain.Interfaces.Services.Account;
using Domain.Models.Api.Auth;
using Shouldly;

namespace Tests_back.Extensions.AccountAuth;

public static class AdminBanTestExtensions
{
  public static async Task<long> BanUserAsync(this TestFixture f, string wallet, string reason, DateTime until)
  {
    var ctrl = f.GetService<AdminController>().WithAdminUser();
    var res = await ctrl.BanUser(new BanUserDto(wallet, reason, until), default);
    return res.OkProp<long>("id");
  }

  public static async Task AssertBannedUntilAsync(this TestFixture f, string wallet, DateTime expectedUntil)
  {
    var accounts = f.GetService<IAccountService>();
    var actual = await accounts.GetBanUntilAsync(wallet, default);

    actual.ShouldNotBeNull();
    actual!.Value.ShouldBe(expectedUntil, TimeSpan.FromSeconds(1));
  }
}