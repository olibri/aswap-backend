using Domain.Interfaces.Services.CoinService;
using Domain.Models.DB.CoinPrice;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace App.Services.CoinPrice;

public sealed class AppLockOptions
{
  public TimeSpan Lease { get; init; } = TimeSpan.FromMinutes(10);
}

public sealed class AppLockService(IDbContextFactory<P2PDbContext> dbFactory, IOptions<AppLockOptions> opt)
  : IAppLockService
{
  public async Task<IAsyncDisposable?> TryAcquireAsync(string lockName, CancellationToken ct)
  {
    var now = DateTime.UtcNow;
    var owner = Guid.NewGuid();
    var until = now.Add(opt.Value.Lease);

    await using var db = await dbFactory.CreateDbContextAsync(ct);

    if (!await db.AppLocks.AsNoTracking().AnyAsync(x => x.Name == lockName, ct))
    {
      db.AppLocks.Add(new AppLockEntity { Name = lockName });
      try
      {
        await db.SaveChangesAsync(ct);
      }
      catch
      {
        Console.WriteLine("");
      }
    }

    var affected = await db.AppLocks
      .Where(x => x.Name == lockName &&
                  (x.LockedUntilUtc == null || x.LockedUntilUtc < now))
      .ExecuteUpdateAsync(setters => setters
          .SetProperty(x => x.LockedUntilUtc, until)
          .SetProperty(x => x.LockOwner, owner),
        ct);

    if (affected == 0)
      return null;

    return new Handle(dbFactory, lockName, owner);
  }

  private sealed class Handle(IDbContextFactory<P2PDbContext> factory, string name, Guid owner)
    : IAsyncDisposable
  {
    public async ValueTask DisposeAsync()
    {
      await using var db = await factory.CreateDbContextAsync();
      var now = DateTime.UtcNow;

      await db.AppLocks
        .Where(x => x.Name == name && x.LockOwner == owner)
        .ExecuteUpdateAsync(setters => setters
          .SetProperty(x => x.LockedUntilUtc, now)
          .SetProperty(x => x.LockOwner, (Guid?)null));
    }
  }
}