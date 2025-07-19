using Domain.Interfaces;
using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

public class OutboxProcessorTask(IServiceScopeFactory scopes, IJsonSerializer json) : IPeriodicTask
{
  public int IntervalSeconds => 1;
  private P2PDbContext db => scopes.CreateScope().ServiceProvider.GetRequiredService<P2PDbContext>();

  public async Task ExecuteAsync(CancellationToken ct)
  {
    var batch = await db.OutboxMessages
      .Where(x => x.ProcessedAt == null)
      .OrderBy(x => x.OccurredAt)
      .Take(100)
      .ToListAsync(ct);

    if (batch.Count == 0) return;

    foreach (var msg in batch)
    {
      db.Events.Add(new EventEntity
      {
        Id = msg.Id,
        Ts = msg.OccurredAt,
        EventType = msg.Type,
        Payload = msg.Payload
      });
      msg.ProcessedAt = DateTime.UtcNow;
    }
    await db.SaveChangesAsync(ct);
  }
}