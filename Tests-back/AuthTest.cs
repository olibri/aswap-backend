using Aswap_back.Controllers;
using Domain.Models.Api.Auth;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;

namespace Tests_back;

public class AuthTest(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task Nonce_Then_Authenticate_Returns_TokenPair()
  {
    var ctrl = fixture.GetService<AuthController>()
      .WithHttp(fixture);

    var w = WalletTestExtensions.CreateWallet();
    var nonce = ctrl.GetNonce(w.Address).OkProp<string>("nonce");
    var pair = await ctrl.AuthenticateOk(new WalletAuthDto("sol", w.Address, nonce, w.Sign(nonce)));

    pair.AssertJwt(w.Address, "user");
  }

}