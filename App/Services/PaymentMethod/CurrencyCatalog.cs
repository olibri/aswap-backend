using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.Api.PaymentMethod;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace App.Services.PaymentMethod;

public sealed class CurrencyCatalog
  : ICurrencyCatalog
{
  private ImmutableArray<CurrencyDto> _cache = [];

  public CurrencyCatalog(IDbContextFactory<P2PDbContext> dbf)
  {
    using var db = dbf.CreateDbContext();
    var data = db.Set<Domain.Models.DB.Currency.Currency>()
      .AsNoTracking()
      .Where(c => c.IsActive)
      .OrderBy(c => c.Name)
      .Select(c => new CurrencyDto(c.Id, c.Code, c.Name))
      .ToList();

    _cache = [.. data];
  }

  public IReadOnlyList<CurrencyDto> All => _cache;


  public IReadOnlyList<CurrencyDto> Search(string query)
  {
    if (string.IsNullOrWhiteSpace(query)) return _cache;

    query = query.Trim();
    return _cache.Where(c =>
        c.Code.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
      .ToArray();
  }
}