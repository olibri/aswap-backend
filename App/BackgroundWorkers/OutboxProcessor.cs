using Domain.Interfaces;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.BackgroundWorkers;

public class OutboxProcessor(IServiceScopeFactory scopeFactory,
  ILogger<OutboxProcessor> log,
  IJsonSerializer serializer)
  : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken ct)
  {
    while (!ct.IsCancellationRequested)
    {
      using var scope = scopeFactory.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

      var batch = await db.OutboxMessages
        .Where(x => x.ProcessedAt == null)
        .OrderBy(x => x.OccurredAt)
        .Take(100)
        .ToListAsync(ct);

      if (batch.Count == 0)
      {
        await Task.Delay(500, ct);
        continue;
      }

      foreach (var msg in batch)
      {
        var evt = new EventEntity
        {
          Id = msg.Id,
          Ts = msg.OccurredAt,
          EventType = msg.Type,
          Payload = msg.Payload,
          // Wallet/IP можна розпарсити з payload, якщо треба
        };
        db.Add(evt);
        msg.ProcessedAt = DateTime.UtcNow;
      }
      await db.SaveChangesAsync(ct);
    }
  }
}
