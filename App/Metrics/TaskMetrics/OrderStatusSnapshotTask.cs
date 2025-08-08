using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

public class OrderStatusSnapshotTask(IServiceScopeFactory scopes) : IPeriodicTask
{
  public int IntervalSeconds => 300;
  public async Task ExecuteAsync(CancellationToken ct)
  {
    await using var scope = scopes.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var today = DateTime.UtcNow.Date;

    var counts = await db.EscrowOrders
      .GroupBy(o => o.EscrowStatus)
      .Select(g => new { Status = g.Key, Cnt = g.Count() })
      .ToListAsync(ct);

    foreach (var row in counts)
    {
      var snap = await db.OrderStatusDaily
        .FindAsync([today, row.Status], ct);

      if (snap is null)
      {
        db.OrderStatusDaily.Add(new OrderStatusDailyEntity
        {
          Day = today,
          Status = row.Status,
          Cnt = row.Cnt
        });
      }
      else
      {
        snap.Cnt = row.Cnt;          
      }
    }

    await db.SaveChangesAsync(ct);
  }
}