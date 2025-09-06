using System.Collections.Concurrent;
using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.DB.PaymentMethod;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Services.PaymentMethod.BackgroundWorker;

public sealed class PopularityCounter(
  IDbContextFactory<P2PDbContext> factory,
  ILogger<PopularityCounter> log)
  : BackgroundService, IPopularityCounter
{
  private ConcurrentDictionary<(short, string), int> _hits = new();

  public void Hit(short[] ids, string region)
  {
    _ = ids.Select(id => _hits.AddOrUpdate((id, region), 1, (_, v) => v + 1));
  }

  public async Task<IReadOnlyList<short>> Top(
    string region, int top, CancellationToken ct)
  {
    await using var db = await factory.CreateDbContextAsync(ct);
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    return await db.PaymentPopularityDaily
      .Where(x => x.Region == region && x.Day == today)
      .OrderByDescending(x => x.Count)
      .Take(top)
      .Select(x => x.MethodId)
      .ToListAsync(ct);
  }

  protected override async Task ExecuteAsync(CancellationToken ct)
  {
    while (!ct.IsCancellationRequested)
    {
      await Task.Delay(TimeSpan.FromSeconds(30), ct);
      await FlushAsync(ct);
    }
  }

  private async Task FlushAsync(CancellationToken ct)
  {
    var current = Interlocked.Exchange(
      ref _hits,
      new ConcurrentDictionary<(short, string), int>());

    var snapshot = current.ToArray();

    if (snapshot.Length == 0) return;

    await using var db = await factory.CreateDbContextAsync(ct);
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    foreach (var ((id, region), cnt) in snapshot)
    {
      var row = await db.PaymentPopularityDaily.FindAsync([today, id, region], ct)
                ?? db.Add(new PaymentPopularityDaily
                  { Day = today, MethodId = id, Region = region, Count = 0 }).Entity;
      row.Count += cnt;
    }

    await db.SaveChangesAsync(ct);
    log.LogDebug("Popularity flushed: {N} rows", snapshot.Length);
  }
}