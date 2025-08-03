using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces.Services.Auth;
using Domain.Models.Api.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace App.Services.Auth;

public sealed class TokenService(IOptions<TokenOptions> opt) : ITokenService
{
  private readonly TokenOptions _opt = opt.Value;

  private readonly SymmetricSecurityKey _key =
    new(Encoding.UTF8.GetBytes(opt.Value.SymmetricKey));

  public TokenPair Generate(string wallet, string role, DateTime? banUntil)
  {
    var now = DateTime.UtcNow;

    var claims = new List<Claim>
    {
      new("sub", wallet),
      new("role", role)
    };
    if (banUntil is not null)
      claims.Add(new Claim("ban_until",
        ((DateTimeOffset)banUntil.Value).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

    var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

    var jwt = new JwtSecurityToken(
      _opt.Issuer,
      _opt.Audience,
      claims,
      now,
      now.AddMinutes(_opt.AccessLifetimeMinutes),
      credentials);

    var access = new JwtSecurityTokenHandler().WriteToken(jwt);
    var refresh = GenerateSecureToken(_opt.RefreshLifetimeDays);
    var refreshExp = DateTime.UtcNow.AddDays(_opt.RefreshLifetimeDays);

    return new TokenPair(access, refresh, jwt.ValidTo, refreshExp);
  }

  private static string GenerateSecureToken(int lifeDays)
  {
    var bytes = RandomNumberGenerator.GetBytes(32);
    return $"{Convert.ToBase64String(bytes)}.{DateTime.UtcNow.AddDays(lifeDays):O}";
  }
}