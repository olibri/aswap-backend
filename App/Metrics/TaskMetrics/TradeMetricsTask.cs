using System.Text.Json;
using Domain.Enums;
using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

public sealed class TradeMetricsTask(IServiceScopeFactory scopes) : IPeriodicTask
{
  public int IntervalSeconds => 5;

  public async Task ExecuteAsync(CancellationToken ct)
  {
    await using var scope = scopes.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var cursor = (await db.AggregatorStates.FindAsync(["trade"], ct))?.Value
                 ?? DateTime.UnixEpoch;

    var batch = await db.Events
      .Where(e => e.EventType == EventType.TradeSettled && e.Ts > cursor)
      .OrderBy(e => e.Ts)
      .Take(1_000)
      .ToListAsync(ct);

    if (batch.Count == 0) return;

    var agg = new Dictionary<(DateTime Day, string Mint), TempAgg>();

    foreach (var ev in batch)
    {
      var p = JsonSerializer.Deserialize<TradeSettledPayload>(ev.Payload)!;

      var order = await db.EscrowOrders.FirstAsync(o => o.DealId == p.DealId, ct);

      var deltaSec = (ev.Ts - order.CreatedAtUtc).TotalSeconds;
      var volume = p.FilledQty * p.PriceFiat; 

      var key = (ev.Ts.Date, p.TokenMint);
      if (!agg.TryGetValue(key, out var a))
        agg[key] = a = new TempAgg();

      a.SumDelta += deltaSec;
      a.SumVolume += volume;
      a.Trades += 1;
    }

    foreach (var ((day, mint), a) in agg)
    {
      var dtd = await db.DealTimeDailyEntity.FindAsync([day, mint], ct);
      if (dtd is null)
      {
        db.DealTimeDailyEntity.Add(new DealTimeDailyEntity
        {
          Day = day,
          TokenMint = mint,
          AvgSeconds = a.SumDelta / a.Trades,
          TradeCnt = a.Trades
        });
      }
      else
      {
        var newCnt = dtd.TradeCnt + a.Trades;
        dtd.AvgSeconds = (dtd.AvgSeconds * dtd.TradeCnt + a.SumDelta) / newCnt;
        dtd.TradeCnt = newCnt;
      }

      var vol = await db.AssetVolumeDaily.FindAsync([day, mint], ct);
      if (vol is null)
        db.AssetVolumeDaily.Add(new AssetVolumeDailyEntity { Day = day, TokenMint = mint, Volume = a.SumVolume });
      else
        vol.Volume += a.SumVolume;
    }

    await db.SaveChangesAsync(ct);

    var state = await db.AggregatorStates.FindAsync(["trade"], ct);
    if (state is null)
      db.AggregatorStates.Add(new AggregatorState { Key = "trade", Value = batch[^1].Ts });
    else
      state.Value = batch[^1].Ts;

    await db.SaveChangesAsync(ct);
  }
}

public sealed class TempAgg
{
  public double SumDelta = 0; 
  public decimal SumVolume = 0; 
  public int Trades = 0;
}

public sealed record TradeSettledPayload(
  ulong DealId,
  string TokenMint,
  decimal FilledQty,
  decimal PriceFiat
);