using Domain.Models.DB;
using Domain.Models.DB.Metrics;
using Infrastructure;

namespace Tests_back.Extensions;

public static class UserMetricsExtensions
{
  public sealed record Expectation(int DauUsers, int WauUsers, int MauUsers, int DauIps);

  /// Створює:  
  ///   • <paramref name="dauUsers"/> активних сьогодні  
  ///   • +<paramref name="wauExtra"/> активних у межах тижня  
  ///   • +<paramref name="mauExtra"/> активних у межах місяця  
  ///   • <paramref name="dauIps"/> різних IP‑адрес
  public static async Task<Expectation> SeedUserMetricsScenarioAsync(
    this P2PDbContext db,
    int dauUsers,
    int wauExtra,
    int mauExtra,
    int dauIps,
    CancellationToken ct = default)
  {
    var today = DateTime.UtcNow.Date;
    var weekAgo = today.AddDays(-3);
    var monthAgo = today.AddDays(-15);

    // Аккаунти
    foreach (var i in Enumerable.Range(1, dauUsers))
      db.Account.Add(new AccountEntity { WalletAddress = $"w{i}", LastActiveTime = today.AddHours(1) });

    foreach (var i in Enumerable.Range(dauUsers + 1, wauExtra))
      db.Account.Add(new AccountEntity { WalletAddress = $"w{i}", LastActiveTime = weekAgo });

    foreach (var i in Enumerable.Range(dauUsers + wauExtra + 1, mauExtra))
      db.Account.Add(new AccountEntity { WalletAddress = $"w{i}", LastActiveTime = monthAgo });

    var now = DateTime.UtcNow;
    foreach (var i in Enumerable.Range(1, dauIps))
      db.Sessions.Add(new SessionEntity
      {
        Wallet = $"w{i}",
        Ip = $"10.0.0.{i}",
        StartedAt = now.AddMinutes(-30),
        LastSeenAt = now
      });

    await db.SaveChangesAsync(ct);

    return new Expectation(
      dauUsers,
      dauUsers + wauExtra,
      dauUsers + wauExtra + mauExtra,
      dauIps);
  }
}