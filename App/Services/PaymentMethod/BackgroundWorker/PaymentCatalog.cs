using System.Collections.Immutable;
using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.Api.PaymentMethod;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Services.PaymentMethod.BackgroundWorker;

public sealed class PaymentCatalog(
  IDbContextFactory<P2PDbContext> factory,
  ILogger<PaymentCatalog> log)
  : BackgroundService, IPaymentCatalog
{
  private ImmutableArray<PaymentDto> _cache = [];

  public IReadOnlyList<PaymentDto> All => _cache;

  public IReadOnlyList<PaymentDto> Search(string q)
  {
    return _cache.Where(m => m.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
      .ToArray();
  }

  protected override async Task ExecuteAsync(CancellationToken ct)
  {
    while (!ct.IsCancellationRequested)
    {
      await ReloadAsync(ct);
      await Task.Delay(TimeSpan.FromMinutes(30), ct);
    }
  }

  public async Task ReloadAsync(CancellationToken ct = default)
  {
    await using var db = await factory.CreateDbContextAsync(ct);
    var data = await db.PaymentMethods
      .Where(m => m.IsActive)
      .Select(m => new PaymentDto(
        m.Id, m.Code, m.Name, m.Category.Name))
      .AsNoTracking()
      .ToListAsync(ct);

    _cache = [..data];
    log.LogInformation("PaymentCatalog reloaded: {Count} items", _cache.Length);
  }
}