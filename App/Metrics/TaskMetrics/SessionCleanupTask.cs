using Domain.Interfaces.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

public sealed class SessionCleanupTask(IServiceScopeFactory scopes) : IPeriodicTask
{
  public int IntervalSeconds => 86_400;

  public async Task ExecuteAsync(CancellationToken ct)
  {
    await using var scope = scopes.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var olderThan = DateTime.UtcNow.AddDays(-1);

    await db.Sessions
      .Where(s => s.LastSeenAt < olderThan)
      .ExecuteDeleteAsync(ct);
  }
}