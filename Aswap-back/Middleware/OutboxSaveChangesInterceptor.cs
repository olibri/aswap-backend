using Domain.Interfaces;
using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Aswap_back.Middleware;

public sealed class OutboxSaveChangesInterceptor(IJsonSerializer json)
  : SaveChangesInterceptor
{
  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    DbContextEventData data, InterceptionResult<int> result, CancellationToken ct = default)
  {
    var ctx = data.Context;

    var domainEvents = ctx.ChangeTracker
      .Entries<IHasDomainEvents>()
      .SelectMany(e => e.Entity.DomainEvents)
      .ToList();

    if (domainEvents.Count > 0)
    {
      var box = ctx.Set<OutboxMessage>();
      foreach (var ev in domainEvents)
      {
        box.Add(new OutboxMessage
        {
          Id = ev.Id,
          Type = ev.Type,
          OccurredAt = ev.OccurredAt,
          Payload = json.ToJson(ev)
        });
      }
    }
    return base.SavingChangesAsync(data, result, ct);
  }

  public override ValueTask<int> SavedChangesAsync(
    SaveChangesCompletedEventData data, int result,
    CancellationToken ct = default)
  {
    var ctx = data.Context;
    foreach (var entry in ctx.ChangeTracker.Entries<IHasDomainEvents>())
      entry.Entity.DomainEvents.Clear();

    return base.SavedChangesAsync(data, result, ct);
  }
}
