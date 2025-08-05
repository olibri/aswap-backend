using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.DB.PaymentMethod;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Tests_back.Extensions.Order;

public static class OrderSeedExt
{
  public static async Task<EscrowOrderEntity> AddOrderAsync(
    this TestFixture f,
    string tokenMint,
    string fiat,
    OrderSide side,
    ulong price,
    EscrowStatus status = EscrowStatus.OnChain,
    IEnumerable<short>? methodIds = null)
  {
    var db = f.GetService<P2PDbContext>();

    var o = new EscrowOrderEntity
    {
      TokenMint = tokenMint,
      FiatCode = fiat,
      OfferSide = side,
      Status = status,
      Price = price,
      PaymentMethods = new List<EscrowOrderPaymentMethodEntity>()
    };

    if (methodIds != null)
    {
      foreach (var mid in methodIds)
        o.PaymentMethods.Add(new EscrowOrderPaymentMethodEntity { MethodId = mid });
    }

    db.EscrowOrders.Add(o);
    await db.SaveChangesAsync();
    return o;
  }

  public static async Task<(PaymentMethod m1, PaymentMethod m2)> AnyTwoMethodsAsync(this TestFixture f)
  {
    var db = f.GetService<P2PDbContext>();
    var list = await db.PaymentMethods.AsNoTracking().Take(2).ToListAsync();
    list.Count.ShouldBeGreaterThanOrEqualTo(2, "payment_methods table must be pre-seeded with >= 2 methods");
    return (list[0], list[1]);
  }
}