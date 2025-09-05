namespace Tests_back.Extensions;

public static class DbContextSeedExtensions
{
  /// <summary>
  /// Додає у контекст випадкові запису PriceSnapshotEntity для заданого tokenMint.
  /// </summary>
  /// <param name="db">Контекст БД (InMemory чи реальна)</param>
  /// <param name="tokenMint">Mint токена, для якого створюються записи</param>
  /// <param name="count">Кількість записів (за замовчуванням 5)</param>
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

    // Вставляємо в DbSet<PriceSnapshotEntity> 
    db.Set<PriceSnapshotEntity>().AddRange(snapshots);

    // Зберігаємо зміни
    await db.SaveChangesAsync();
  }
}