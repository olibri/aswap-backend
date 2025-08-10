using System.Text.Json;
using Domain.Enums;
using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

public sealed class OrderMetricsTask(IServiceScopeFactory scopes) : IPeriodicTask
{
  public int IntervalSeconds => 1;
  private P2PDbContext Db => scopes.CreateScope().ServiceProvider.GetRequiredService<P2PDbContext>();

  public async Task ExecuteAsync(CancellationToken ct)
  {
    await using var scope = scopes.CreateAsyncScope();

    var since = await LoadCursorAsync(ct);
    var batch = await LoadBatchAsync(since, ct);
    if (batch.Count == 0) return;

    var groups = GroupBatch(batch);

    await UpsertSnapshotsAsync(groups, ct);
    await MoveCursorAsync(batch[^1].Ts, ct);
  }

  private async Task<DateTime> LoadCursorAsync(CancellationToken ct)
  {
    return (await Db.AggregatorStates.FindAsync(["orders"], ct))?.Value
           ?? DateTime.UnixEpoch;
  }

  private Task<List<EventEntity>> LoadBatchAsync(DateTime since, CancellationToken ct)
  {
    return Db.Events.Where(e => e.EventType == EventType.OfferCreated && e.Ts > since)
      .OrderBy(e => e.Ts)
      .Take(1_000)
      .ToListAsync(ct);
  }

  private static IEnumerable<(DateTime Day, OrderSide Side, int Cnt)> GroupBatch(IEnumerable<EventEntity> batch)
  {
    return batch.GroupBy(e =>
      {
        var oc = JsonSerializer.Deserialize<OfferCreated>(e.Payload)!;
        return (Day: e.Ts.Date, oc.Side);
      })
      .Select(g => (g.Key.Day, g.Key.Side, g.Count()));
  }

  private async Task UpsertSnapshotsAsync(IEnumerable<(DateTime Day, OrderSide Side, int Cnt)> groups,
    CancellationToken ct)
  {
    foreach (var (day, side, cnt) in groups)
    {
      var snap = await Db.OrderCreatedDaily.FindAsync([day, side], ct);
      if (snap is null)
        Db.OrderCreatedDaily.Add(new OrderCreatedDailyEntity { Day = day, Side = side, CreatedCnt = cnt });
      else
        snap.CreatedCnt += cnt;
    }

    await Db.SaveChangesAsync(ct);
  }

  private async Task MoveCursorAsync(DateTime lastTs, CancellationToken ct)
  {
    var state = await Db.AggregatorStates.FindAsync(["orders"], ct);
    if (state is null)
      Db.AggregatorStates.Add(new AggregatorState { Key = "orders", Value = lastTs });
    else
      state.Value = lastTs;

    await Db.SaveChangesAsync(ct);
  }
}