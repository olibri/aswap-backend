using Domain.Interfaces.Services.CoinService.TokenRepo;
using Domain.Models.Api.CoinPrice;
using Domain.Models.DB.CoinPrice;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.CoinPrice.TokenRepo;

public sealed class PriceSnapshotRepository(P2PDbContext db) : IPriceSnapshotRepository
{
  public async Task UpsertMinuteAsync(IEnumerable<PriceSnapshotUpsertDto> rows, CancellationToken ct)
  {
    var latest = rows
        .GroupBy(r => new { r.TokenMint, r.Quote, r.MinuteBucketUtc })
        .ToDictionary(g => g.Key, g => g.MaxBy(x => x.CollectedAtUtc)!);

    if (latest.Count == 0) return;

    var mints = latest.Keys.Select(k => k.TokenMint).Distinct().ToArray();
    var quotes = latest.Keys.Select(k => k.Quote).Distinct().ToArray();
    var buckets = latest.Keys.Select(k => k.MinuteBucketUtc).Distinct().ToArray();

    var existingKeys = await db.Set<PriceSnapshotEntity>()
        .Where(x => mints.Contains(x.TokenMint)
                 && buckets.Contains(x.MinuteBucketUtc))
        .Select(x => new { x.TokenMint, x.MinuteBucketUtc })
        .AsNoTracking()
        .ToListAsync(ct);

    var existSet = existingKeys
        .Select(k => (k.TokenMint, k.MinuteBucketUtc))
        .ToHashSet();

    // INSERT-и
    var toInsert = latest
        .Where(kv => !existSet.Contains((kv.Key.TokenMint, kv.Key.MinuteBucketUtc)))
        .Select(kv => new PriceSnapshotEntity
        {
          TokenMint = kv.Key.TokenMint,
          MinuteBucketUtc = kv.Key.MinuteBucketUtc,
          Price = kv.Value.Price,
          CollectedAtUtc = kv.Value.CollectedAtUtc
        })
        .ToList();

    if (toInsert.Count > 0)
      await db.Set<PriceSnapshotEntity>().AddRangeAsync(toInsert, ct);

    foreach (var (key, dto) in latest.Where(kv => existSet.Contains((kv.Key.TokenMint, kv.Key.MinuteBucketUtc))))
    {
      await db.Set<PriceSnapshotEntity>()
          .Where(x => x.TokenMint == key.TokenMint
                   && x.MinuteBucketUtc == key.MinuteBucketUtc)
          .ExecuteUpdateAsync(set => set
              .SetProperty(x => x.Price, dto.Price)
              .SetProperty(x => x.CollectedAtUtc, dto.CollectedAtUtc), ct);
    }

    if (toInsert.Count > 0)
      await db.SaveChangesAsync(ct);
  }

  public Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct)
    => db.Set<PriceSnapshotEntity>()
         .Where(x => x.MinuteBucketUtc < cutoffUtc)
         .ExecuteDeleteAsync(ct);

  public Task<int> DeleteAllExceptDayAsync(DateOnly dayUtc, CancellationToken ct)
  {
    var start = dayUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    var end = start.AddDays(1);

    return db.Set<PriceSnapshotEntity>()
      .Where(x => x.MinuteBucketUtc < start || x.MinuteBucketUtc >= end)
      .ExecuteDeleteAsync(ct);
  }
}