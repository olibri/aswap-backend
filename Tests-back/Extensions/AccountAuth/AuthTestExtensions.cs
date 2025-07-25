using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Aswap_back.Controllers;
using Domain.Models.Api.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Tests_back.Extensions.AccountAuth;

public static class AuthTestExtensions
{
  public static TProp OkProp<TProp>(this IActionResult result, string propName)
  {
    result.ShouldBeOfType<OkObjectResult>();
    var ok = (OkObjectResult)result;
    ok.Value.ShouldNotBeNull();

    var prop = ok.Value!.GetType().GetProperty(propName);
    prop.ShouldNotBeNull($"Property '{propName}' not found.");
    return (TProp)prop!.GetValue(ok.Value)!;
  }

  public static async Task<TokenPair> AuthenticateOk(
    this AuthController ctrl, WalletAuthDto dto, CancellationToken ct = default)
  {
    var res = await ctrl.Authenticate(dto, ct);
    res.ShouldBeOfType<OkObjectResult>();
    var pair = ((OkObjectResult)res).Value.ShouldBeOfType<TokenPair>();
    pair.AccessToken.ShouldNotBeNullOrWhiteSpace();
    pair.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    return pair;
  }

  public static void AssertJwt(this TokenPair pair, string expectedSub, string expectedRole)
  {
    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pair.AccessToken);
    jwt.Claims.First(c => c.Type == "sub").Value.ShouldBe(expectedSub);
    jwt.Claims.First(c => c.Type == "role").Value.ShouldBe(expectedRole);
  }

  public static T WithAdminUser<T>(this T ctrl) where T : ControllerBase
  {
    var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
      new Claim("sub", "admin_wallet"),
      new Claim("role", "admin")
    }, "TestAuth"));

    ctrl.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext { User = user }
    };
    return ctrl;
  }
}