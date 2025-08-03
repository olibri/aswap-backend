using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces.Services.Auth;
using Domain.Models.Api.Auth;
using Domain.Models.DB;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Auth;

public sealed class RefreshTokenService(P2PDbContext db) : IRefreshTokenService
{
  public async Task SaveAsync(SaveRefreshDto dto, CancellationToken ct)

  {
    db.Set<RefreshTokenEntity>().Add(new RefreshTokenEntity
    {
      Wallet = dto.Wallet,
      RefreshHash = Hash(dto.RefreshToken),
      ExpiresAtUtc = dto.ExpiresUtc,
      CreatedAtUtc = DateTime.UtcNow,
      UserAgent = dto.UserAgent,
      Ip = dto.Ip
    });
    await db.SaveChangesAsync(ct);
  }

  public async Task<(long Id, string Wallet)?> ValidateAsync(string refresh, CancellationToken ct)
  {
    if (!TryParseRefresh(refresh, out var exp) || exp <= DateTime.UtcNow) return null;

    var hash = Hash(refresh);
    var row = await db.Set<RefreshTokenEntity>()
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.RefreshHash == hash && x.RevokedAtUtc == null, ct);

    return row is null ? null : (row.Id, row.Wallet);
  }

  public async Task RotateAsync(RotateRefreshDto dto, CancellationToken ct)
  {
    await db.Set<RefreshTokenEntity>()
      .Where(x => x.Id == dto.SessionId && x.RefreshHash == Hash(dto.OldRefreshToken))
      .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAtUtc, DateTime.UtcNow), ct);

    db.Set<RefreshTokenEntity>().Add(new RefreshTokenEntity
    {
      Wallet =
        await db.Set<RefreshTokenEntity>().Where(x => x.Id == dto.SessionId)
          .Select(x => x.Wallet).FirstAsync(ct),
      RefreshHash = Hash(dto.NewRefreshToken),
      ExpiresAtUtc = dto.NewExpiresUtc,
      CreatedAtUtc = DateTime.UtcNow,
      UserAgent = dto.UserAgent,
      Ip = dto.Ip
    });
    await db.SaveChangesAsync(ct);
  }

  public async Task RevokeAsync(long sessionId, CancellationToken ct)
  {
    await db.Set<RefreshTokenEntity>()
      .Where(x => x.Id == sessionId)
      .ExecuteUpdateAsync(s
        => s.SetProperty(x => x.RevokedAtUtc, DateTime.UtcNow), ct);
  }

  private static string Hash(string token)
  {
    using var sha = SHA256.Create();
    return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
  }

  public static bool TryParseRefresh(string token, out DateTime expiresUtc)
  {
    expiresUtc = default;
    var parts = token.Split('.', 2);
    return parts.Length == 2 &&
           DateTime.TryParse(parts[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out expiresUtc);
  }
}