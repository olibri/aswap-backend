using Domain.Interfaces.Services.CoinService.TokenRepo;
using Domain.Models.Api.CoinPrice;
using Domain.Models.DB.CoinPrice;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.CoinPrice.TokenRepo;

public sealed class TokenRepository(IDbContextFactory<P2PDbContext> factory) : ITokenRepository
{
  public async Task<bool> AnyAsync(CancellationToken ct)
  {
    await using var db = await factory.CreateDbContextAsync(ct);
    return await db.Tokens.AsNoTracking().AnyAsync(ct);
  }

  public async Task UpsertAsync(IEnumerable<TokenDto> tokens, CancellationToken ct)
  {
    await using var db = await factory.CreateDbContextAsync(ct);

    var list = tokens
      .Where(t => !string.IsNullOrWhiteSpace(t.Mint))
      .GroupBy(t => t.Mint)
      .Select(g => g.Last())
      .ToList();

    if (list.Count == 0) return;

    var mints = list.Select(t => t.Mint).Distinct().ToArray();
    var existing = await db.Tokens
      .Where(x => mints.Contains(x.Mint))
      .ToDictionaryAsync(x => x.Mint, ct);

    var toInsert = new List<TokenEntity>();
    foreach (var t in list)
    {
      if (existing.TryGetValue(t.Mint, out var e))
      {
        e.Symbol = t.Symbol;
        e.Name = t.Name;
        e.Decimals = t.Decimals;
        e.IsVerified = t.IsVerified;
        e.Icon = t.Icon;
      }
      else
      {
        toInsert.Add(new TokenEntity
        {
          Mint = t.Mint,
          Symbol = t.Symbol,
          Name = t.Name,
          Decimals = t.Decimals,
          IsVerified = t.IsVerified,
          Icon = t.Icon
        });
      }
    }

    if (toInsert.Count > 0)
      await db.Tokens.AddRangeAsync(toInsert, ct);

    await db.SaveChangesAsync(ct);
  }

  public async Task<IReadOnlyList<string>> GetAllMintsAsync(CancellationToken ct)
  {
    await using var db = await factory.CreateDbContextAsync(ct);
    return await db.Tokens.AsNoTracking()
      .Select(x => x.Mint)
      .OrderBy(x => x)
      .ToListAsync(ct);
  }

  public async Task<TokenDto?> GetByMintAsync(string mint, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(mint))
      return null;

    await using var db = await factory.CreateDbContextAsync(ct);
    var entity = await db.Tokens.AsNoTracking()
      .FirstOrDefaultAsync(x => x.Mint == mint, ct);

    return entity == null ? null : new TokenDto(
      entity.Mint,
      entity.Symbol,
      entity.Name,
      entity.Decimals,
      entity.IsVerified,
      entity.Icon
    );

  }
}