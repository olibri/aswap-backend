using App.Utils;
using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

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

    var tvl = TvlUtils.Calculate(await db.EscrowOrders.ToListAsync(ct));

    foreach (var (mint, balance) in tvl)
      db.TvlSnapshots.Add(new TvlSnapshotEntity
      {
        TakenAt = now,
        TokenMint = mint,
        Balance = balance
      });

    await db.SaveChangesAsync(ct);
  }
}