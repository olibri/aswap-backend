using Domain.Models.DB.CoinPrice;
using Microsoft.EntityFrameworkCore;

namespace Tests_back.Extensions;

public static class DbContextSeedExtensions
{
  public static async Task SeedPriceSnapshotsAsync(
    this DbContext db,
    string tokenMint,
    int count = 5)
  {
    var now = DateTime.UtcNow;
    var rnd = new Random();

    var snapshots = Enumerable.Range(0, count)
      .Select(i => new PriceSnapshotEntity
      {
        Id = Guid.NewGuid(),
        TokenMint = tokenMint,
        Price = Math.Round((decimal)(rnd.NextDouble() * 100), 18),
        MinuteBucketUtc = now.AddMinutes(-i),
        CollectedAtUtc = now.AddMinutes(-i)
          .AddSeconds(rnd.Next(0, 60))
      })
      .ToList();

    db.Set<PriceSnapshotEntity>().AddRange(snapshots);

    await db.SaveChangesAsync();
  }
}