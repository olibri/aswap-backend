using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

  public static T WithHttp<T>(this T ctrl, TestFixture f, string ua = "Tests/1.0", string ip = "127.0.0.1")
    where T : ControllerBase
  {
    var http = new DefaultHttpContext();
    http.Request.Headers["User-Agent"] = ua;
    http.Connection.RemoteIpAddress = IPAddress.Parse(ip);

    ctrl.ControllerContext = new ControllerContext { HttpContext = http };

    var accessor = f.GetService<IHttpContextAccessor>();
    accessor.HttpContext = http;

    return ctrl;
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

  public static T WithUser<T>(this T ctrl, string wallet, string role = "user") where T : ControllerBase
  {
    var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
      new Claim("sub", wallet),
      new Claim("role", role)
    }, "TestAuth"));

    ctrl.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext { User = user }
    };
    return ctrl;
  }

  public static T WithAdminUser<T>(this T ctrl) where T : ControllerBase
  {
    return ctrl.WithUser("admin_wallet", "admin");
  }
}