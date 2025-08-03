using Aswap_back.Controllers;
using Domain.Models.Api.Auth;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;

namespace Tests_back;

public class AuthTest(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task Nonce_Then_Authenticate_Sets_RefreshCookie_And_ReturnsPair()
  {
    var ctrl = fixture.GetService<AuthController>().WithHttp(fixture);
    var w = WalletTestExtensions.CreateWallet();

    var nonce = ctrl.GetNonce(w.Address).OkProp<string>("nonce");
    var pair = await ctrl.AuthenticateOk(new WalletAuthDto("sol", w.Address, nonce, w.Sign(nonce)));

    pair.AssertJwt(w.Address, "user");
    ctrl.GetCookie("refresh_token").ShouldNotBeNullOrWhiteSpace();
  }

  [Fact]
  public async Task Refresh_Uses_Cookie_And_Rotates_It()
  {
    var ctrl = fixture.GetService<AuthController>().WithHttp(fixture);
    var w = WalletTestExtensions.CreateWallet();

    var nonce = ctrl.GetNonce(w.Address).OkProp<string>("nonce");
    await ctrl.AuthenticateOk(new WalletAuthDto("sol", w.Address, nonce, w.Sign(nonce)));
    var oldRt = ctrl.GetCookie("refresh_token");

    ctrl.WithHttp(fixture).WithRequestCookie("refresh_token", oldRt);
    var refreshed = await ctrl.RefreshOk();

    refreshed.AccessToken.ShouldNotBeNullOrWhiteSpace();
    var newRt = ctrl.GetCookie("refresh_token");
    newRt.ShouldNotBeNullOrWhiteSpace();
    newRt.ShouldNotBe(oldRt);
  }

  [Fact]
  public async Task Logout_Clears_Cookie_And_Refresh_Fails()
  {
    var ctrl = fixture.GetService<AuthController>().WithHttp(fixture);
    var w = WalletTestExtensions.CreateWallet();

    var nonce = ctrl.GetNonce(w.Address).OkProp<string>("nonce");
    await ctrl.AuthenticateOk(new WalletAuthDto("sol", w.Address, nonce, w.Sign(nonce)));
    var rt = ctrl.GetCookie("refresh_token");

    ctrl.WithHttp(fixture).WithRequestCookie("refresh_token", rt);
    await ctrl.LogoutOk();

    ctrl.WithHttp(fixture).WithRequestCookie("refresh_token", rt);
    await ctrl.RefreshUnauthorized();
  }
}