using Domain.Interfaces.Services;
using Domain.Interfaces.Services.IP;
using Domain.Models.Api;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Sessions;

public sealed class SessionService(
  P2PDbContext db,
  IAccountService accounts,
  IClientIpAccessor ipAccessor) : ISessionService
{
  public async Task StartAsync(SessionPingDto dto, CancellationToken ct)
  {
    await UpsertSessionAsync(dto, ct);
    await accounts.TouchAsync(dto.Wallet, ct);
  }

  public async Task PingAsync(SessionPingDto dto, CancellationToken ct)
  {
    var rows = await db.Sessions
      .Where(s => s.SessionId == dto.SessionId)
      .ExecuteUpdateAsync(s =>
        s.SetProperty(x => x.LastSeenAt, DateTime.UtcNow), ct);

    if (rows == 0)
      await UpsertSessionAsync(dto, ct);

    await accounts.TouchAsync(dto.Wallet, ct);
  }

  private async Task UpsertSessionAsync(SessionPingDto dto, CancellationToken ct)
  {
    var now = DateTime.UtcNow;
    var ip = ipAccessor.GetClientIp();

    var exists = await db.Sessions
      .AnyAsync(s => s.SessionId == dto.SessionId, ct);

    if (!exists)
      db.Sessions.Add(new SessionEntity
      {
        SessionId = dto.SessionId,
        Wallet = dto.Wallet,
        Ip = ip,
        StartedAt = now,
        LastSeenAt = now
      });
    else
      await db.Sessions
        .Where(s => s.SessionId == dto.SessionId)
        .ExecuteUpdateAsync(s =>
          s.SetProperty(x => x.Ip, ip)
            .SetProperty(x => x.LastSeenAt, now), ct);

    await db.SaveChangesAsync(ct);
  }
}