using Domain.Enums;
using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics;

public class TvlSnapshotTask(IServiceScopeFactory scopes) : IPeriodicTask
{
  public int IntervalSeconds => 300;

  public async Task ExecuteAsync(CancellationToken ct)
  {
    await using var scope = scopes.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var now = DateTime.UtcNow
      .AddSeconds(-DateTime.UtcNow.Second)
      .AddMilliseconds(-DateTime.UtcNow.Millisecond);

    var tvl = await db.EscrowOrders
      .Where(o => o.Status == EscrowStatus.OnChain ||
                  o.Status == EscrowStatus.PartiallyOnChain)
      .GroupBy(o => o.TokenMint)
      .Select(g => new { g.Key, Locked = g.Sum(x => (x.Amount ?? 0) - x.FilledQuantity) })
      .ToListAsync(ct);

    foreach (var row in tvl)
      db.TvlSnapshots.Add(new TvlSnapshotEntity
      {
        TakenAt = now,
        TokenMint = row.Key!,
        Balance = row.Locked
      });

    await db.SaveChangesAsync(ct);
  }
}
