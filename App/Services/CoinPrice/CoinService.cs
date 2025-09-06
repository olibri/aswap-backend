using Domain.Interfaces.Services.CoinService;
using Domain.Models.Api.CoinPrice;
using Domain.Models.DB.CoinPrice;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.CoinPrice;

public class CoinService(IDbContextFactory<P2PDbContext> dbFactory) : ICoinService
{
  public async Task<TokenDailyPriceResponse[]> GetPricesAsync(
    string coinX,
    string coinY)
  {
    var mints = NormalizeMints(coinX, coinY);
    if (mints.Length == 0)
      return [];

    var (fromUtc, nowUtc) = GetWindowUtc();
    await using var db = await dbFactory.CreateDbContextAsync();

    var rows = await LoadRowsAsync(db, mints, fromUtc, nowUtc);
    var byMint = GroupByMint(rows);
    var dateUtc = DateOnly.FromDateTime(nowUtc);

    var result = mints.Select(mint =>
    {
      var list = byMint.TryGetValue(mint, out var v) ? v : [];
      return MapResponse(mint, list, dateUtc);
    }).ToArray();

    return result;
  }

  public async Task<(decimal, decimal)> GetLastPriceAsync(string coinX, string coinY)
  {
    var mints = NormalizeMints(coinX, coinY);
    if (mints.Length == 0)
      return (0m, 0m);

    await using var db = await dbFactory.CreateDbContextAsync();

    var coinXPrice = await GetLatestPriceAsync(mints[0], db);
    var coinYPrice = await GetLatestPriceAsync(mints[1], db);


    return (coinXPrice.Price, coinYPrice.Price);
  }

  private async Task<PriceSnapshotEntity> GetLatestPriceAsync(
    string tokenMint,
    P2PDbContext db,
    CancellationToken cancellationToken = default)
  {
    var snapshot = await db.PriceSnapshot
      .AsNoTracking()
      .Where(p => p.TokenMint == tokenMint)
      .OrderByDescending(p => p.CollectedAtUtc)
      .FirstOrDefaultAsync(cancellationToken);

    if (snapshot is null)
      throw new InvalidOperationException($"Price not found for mint '{tokenMint}'");

    return snapshot;
  }


  public async Task<TokenDto[]> GetCoinsAsync(CancellationToken ct)
  {
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var tokens = await db.Set<TokenEntity>()
      .AsNoTracking()
      .OrderBy(t => t.Symbol ?? t.Name ?? t.Mint) // зручне сортування
      .Select(t => new TokenDto(
        t.Mint,
        t.Symbol,
        t.Name,
        t.Decimals,
        t.IsVerified,
        t.Icon
      ))
      .ToArrayAsync(ct);

    return tokens;
  }


  private static string[] NormalizeMints(string coinX, string coinY)
  {
    return new[] { coinX, coinY }
      .Where(s => !string.IsNullOrWhiteSpace(s))
      .Distinct(StringComparer.Ordinal)
      .ToArray();
  }

  private static (DateTime fromUtc, DateTime nowUtc) GetWindowUtc()
  {
    var nowUtc = DateTime.UtcNow;
    var fromUtc = nowUtc.AddHours(-24);
    return (fromUtc, nowUtc);
  }

  private static async Task<List<PriceSnapshotEntity>> LoadRowsAsync(
    P2PDbContext db,
    string[] mints,
    DateTime fromUtc,
    DateTime nowUtc)
  {
    return await db.Set<PriceSnapshotEntity>()
      .AsNoTracking()
      .Where(x =>
        mints.Contains(x.TokenMint) &&
        x.MinuteBucketUtc >= fromUtc &&
        x.MinuteBucketUtc <= nowUtc)
      .OrderBy(x => x.TokenMint)
      .ThenBy(x => x.MinuteBucketUtc)
      .ToListAsync();
  }

  private static Dictionary<string, List<PriceSnapshotEntity>> GroupByMint(
    List<PriceSnapshotEntity> rows)
  {
    return rows.GroupBy(r => r.TokenMint)
      .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
  }

  private static TokenDailyPriceResponse MapResponse(
    string mint,
    List<PriceSnapshotEntity> list,
    DateOnly dateUtc)
  {
    var points = list.Select(MapPoint).ToArray();
    var firstCollected = list.Count > 0 ? list.Min(r => r.CollectedAtUtc) : (DateTime?)null;
    var lastCollected = list.Count > 0 ? list.Max(r => r.CollectedAtUtc) : (DateTime?)null;

    return new TokenDailyPriceResponse
    {
      TokenMint = mint,
      DateUtc = dateUtc,
      Points = points,
      FirstCollectedUtc = firstCollected,
      LastCollectedUtc = lastCollected
    };
  }

  private static PricePointDto MapPoint(PriceSnapshotEntity r)
  {
    return new PricePointDto
    {
      Time = new DateTimeOffset(
        r.MinuteBucketUtc.Kind == DateTimeKind.Utc
          ? r.MinuteBucketUtc
          : DateTime.SpecifyKind(r.MinuteBucketUtc, DateTimeKind.Utc)),
      Price = r.Price
    };
  }
}