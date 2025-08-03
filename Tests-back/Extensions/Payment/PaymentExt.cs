using Domain.Interfaces.Services.PaymentMethod;
using Domain.Models.DB.PaymentMethod;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Tests_back.Extensions.Payment;

public static class PaymentExt
{
  /// <summary>Add (or increment) popularity counter for today.</summary>
  public static async Task AddPopularityAsync(
    this TestFixture f,
    short methodId,
    string region = "ZZ",
    int hits = 1)
  {
    var db = f.GetService<P2PDbContext>();
    var day = DateOnly.FromDateTime(DateTime.UtcNow);

    var row = await db.PaymentPopularityDaily.FindAsync(day, methodId, region)
              ?? db.PaymentPopularityDaily.Add(new PaymentPopularityDaily
                { Day = day, MethodId = methodId, Region = region, Count = 0 }).Entity;

    row.Count += hits;
    await db.SaveChangesAsync();
  }

  /// <summary>Get any active payment-method for tests.</summary>
  public static async Task<PaymentMethod> AnyMethodAsync(this TestFixture f)
  {
    var db = f.GetService<P2PDbContext>();
    var m = await db.PaymentMethods.AsNoTracking().FirstAsync();
    m.ShouldNotBeNull("payment_methods table must be pre-seeded");
    return m;
  }

  /// <summary>Reload in-memory catalog (so controller sees fresh data).</summary>
  public static async Task ReloadCatalogAsync(this TestFixture f)
  {
    var cat = f.GetService<IPaymentCatalog>();
    await cat.ReloadAsync();
  }
}