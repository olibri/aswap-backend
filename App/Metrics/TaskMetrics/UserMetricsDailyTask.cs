using Domain.Interfaces.Metrics;
using Domain.Models.DB.Metrics;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.TaskMetrics;

public sealed class UserMetricsDailyTask(IServiceScopeFactory scopes) : IPeriodicTask
{
  public int IntervalSeconds => 86_400;

  public async Task ExecuteAsync(CancellationToken ct)
  {
    await using var scope = scopes.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<P2PDbContext>();

    var today = DateTime.UtcNow.Date;
    var weekAgo = today.AddDays(-6);
    var monthAgo = today.AddDays(-29);

    var dauUsers = await db.Account
      .CountAsync(a => a.LastActiveTime >= today, ct);

    var wauUsers = await db.Account
      .CountAsync(a => a.LastActiveTime >= weekAgo, ct);

    var mauUsers = await db.Account
      .CountAsync(a => a.LastActiveTime >= monthAgo, ct);

    var dauIps = await db.Sessions
      .Where(s => s.LastSeenAt >= today)
      .Select(s => s.Ip!)
      .Distinct()
      .CountAsync(ct);

    var snap = await db.UserMetricsDaily.FindAsync([today], ct);

    if (snap is null)
    {
      db.UserMetricsDaily.Add(new UserMetricsDailyEntity
      {
        Day = today,
        DauUsers = dauUsers,
        DauIps = dauIps,
        WauUsers = wauUsers,
        MauUsers = mauUsers
      });
    }
    else
    {
      snap.DauUsers = dauUsers;
      snap.DauIps = dauIps;
      snap.WauUsers = wauUsers;
      snap.MauUsers = mauUsers;
    }

    await db.SaveChangesAsync(ct);
  }
}