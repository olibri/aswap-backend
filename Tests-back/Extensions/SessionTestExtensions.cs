using Domain.Models.Api;
using Domain.Models.DB;
using Infrastructure;

namespace Tests_back.Extensions;

public static class SessionTestExtensions
{
  public static async Task<SessionPingDto> SeedAccountAsync(
    this P2PDbContext db,
    string wallet,
    CancellationToken ct = default)
  {
    db.Account.Add(new AccountEntity
    {
      WalletAddress = wallet,
      LastActiveTime = DateTime.UtcNow.AddDays(-1)   
    });
    await db.SaveChangesAsync(ct);

    return new SessionPingDto(Guid.NewGuid(), wallet);
  }
}