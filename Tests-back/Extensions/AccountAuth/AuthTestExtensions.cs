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

  public static T WithRequestCookie<T>(this T ctrl, string name, string value) where T : ControllerBase
  {
    // Достатньо виставити заголовок Cookie
    ctrl.HttpContext.Request.Headers.Append("Cookie", $"{name}={value}");
    return ctrl;
  }

  public static string? GetCookie(this ControllerBase ctrl, string name)
  {
    // Шукаємо у всіх Set-Cookie (може бути кілька)
    var setCookies = ctrl.HttpContext.Response.Headers["Set-Cookie"].ToArray();
    if (setCookies.Length == 0) return null;

    foreach (var sc in setCookies)
    {
      // формат: name=value; Path=/; HttpOnly; ...
      var parts = sc.Split(';', 2);
      var kv = parts[0].Split('=', 2);
      if (kv.Length == 2 && string.Equals(kv[0].Trim(), name, StringComparison.OrdinalIgnoreCase))
        return kv[1];
    }

    return null;
  }

  public static async Task<TokenPair> RefreshOk(this AuthController ctrl, CancellationToken ct = default)
  {
    var res = await ctrl.Refresh(ct);
    res.ShouldBeOfType<OkObjectResult>();
    var pair = ((OkObjectResult)res).Value.ShouldBeOfType<TokenPair>();
    pair.AccessToken.ShouldNotBeNullOrWhiteSpace();
    pair.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    return pair;
  }

  public static async Task LogoutOk(this AuthController ctrl, CancellationToken ct = default)
  {
    var res = await ctrl.Logout(ct);
    res.ShouldBeOfType<OkResult>();
  }

  public static async Task RefreshUnauthorized(this AuthController ctrl, CancellationToken ct = default)
  {
    var res = await ctrl.Refresh(ct);
    res.ShouldBeOfType<UnauthorizedObjectResult>();
  }
}