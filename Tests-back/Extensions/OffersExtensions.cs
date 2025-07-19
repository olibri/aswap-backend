using System.Text.Json;
using App.Metrics.TaskMetrics;
using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Models.DB;
using Domain.Models.DB.Metrics;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Tests_back.Extensions;

public static class OffersExtensions
{
  private static readonly Random Rnd = new();

  public static async Task<IActionResult> CreateFakeOrder(TestFixture fixture)
  {
    var controller = fixture.GetService<WebHookController>();

    controller.SetJsonBody(TestJson.OfferInitialized);
    var token = new CancellationToken();

    var result = await controller.QuickNodeCallback(token);

    return result;
  }

  public static async Task CreateFakeOrders(TestFixture fixture, int ordersCount)
  {
    for (var i = 0; i < ordersCount; i++) await CreateFakeOrder(fixture);
  }

  public static List<EscrowOrderEntity> Generate(
    int count,
    EscrowStatus? status = null,
    string? tokenMint = null,
    OrderSide? side = null,
    bool partialFill = false,
    int? fixedAmount = null)
  {
    var list = new List<EscrowOrderEntity>(count);
    for (var i = 0; i < count; i++)
    {
      var amt = fixedAmount ?? Rnd.Next(1, 1000);
      var filled = partialFill ? Rnd.Next(0, amt) : 0;

      list.Add(new EscrowOrderEntity
      {
        Id = Guid.NewGuid(),
        EscrowPda = $"EscrowPda{i:D4}",
        DealId = (ulong)Rnd.NextInt64(1_000_000_000_000),
        SellerCrypto = $"wallet_{Rnd.Next(1, 100)}",
        BuyerFiat = null,
        TokenMint = tokenMint ?? (Rnd.Next(2) == 0 ? "USDc" : "So11111111111111111111111111111111111111112"),
        FiatCode = "USD",
        Amount = (ulong)amt,
        Price = (ulong)Rnd.Next(100, 10_000),
        Status = status ?? (EscrowStatus)Rnd.Next(Enum.GetValues<EscrowStatus>().Length),
        CreatedAtUtc = DateTime.UtcNow.AddMinutes(-Rnd.Next(0, 10_000)),
        OfferSide = side ?? (Rnd.Next(2) == 0 ? OrderSide.Sell : OrderSide.Buy),
        MinFiatAmount = 10,
        MaxFiatAmount = 10000,
        FilledQuantity = filled
      });
    }

    return list;
  }

  public static async Task<List<EscrowOrderEntity>> SeedAsync(
    this P2PDbContext db,
    int count,
    EscrowStatus? status = null,
    string? tokenMint = null,
    OrderSide? side = null,
    bool partialFill = false,
    CancellationToken ct = default,
    int? fixedAmount = null)
  {
    var list = Generate(count, status, tokenMint, side, partialFill, fixedAmount);
    await db.EscrowOrders.AddRangeAsync(list, ct);
    await db.SaveChangesAsync(ct);
    return list;
  }

  public static decimal ExpectedTvl(
    this IEnumerable<EscrowOrderEntity> orders,
    string tokenMint)
  {
    return orders
      .Where(o => o.TokenMint == tokenMint &&
                  (o.Status is EscrowStatus.OnChain or EscrowStatus.PartiallyOnChain))
      .Sum(o => (o.Amount ?? 0m) - o.FilledQuantity);
  }


  /// <summary>
  /// ??????? <paramref name="count"/> ??????? ? ??????? Released ?
  /// ?????????? ????? <c>TradeSettled</c> ? <see cref="EventEntity"/>.
  /// ???????? ???????? ?????? — ? ??? ?????? ??????????? ????????? ???????.
  /// </summary>
  public static async Task<List<EscrowOrderEntity>> SeedTradesAsync(
      this P2PDbContext db,
      int count,
      DateTime settledTs,
      string tokenMint = "USDc",
      decimal qtyEach = 100m,
      decimal priceFiat = 1m,
      CancellationToken ct = default)
  {
    var orders = Generate(
        count,
        EscrowStatus.Released,
        tokenMint,
        OrderSide.Sell,
        partialFill: false,
        fixedAmount: (int)qtyEach);

    foreach (var o in orders)
    {
      o.CreatedAtUtc = settledTs.AddMinutes(-Rnd.Next(5, 60)); // 5?60 ?? ?? ?????
      o.FilledQuantity = qtyEach;
      o.Price = (ulong)priceFiat;
    }

    var events = orders.Select(o => new EventEntity
    {
      Ts = settledTs,
      EventType = EventType.TradeSettled,
      Payload = JsonSerializer.Serialize(
            new TradeSettledPayload(o.DealId, tokenMint, qtyEach, priceFiat))
    });

    await db.EscrowOrders.AddRangeAsync(orders, ct);
    await db.Events.AddRangeAsync(events, ct);
    await db.SaveChangesAsync(ct);
    return orders;
  }

  /// <summary>????????? ???????? (avg ? ???, volume, trades) ??? ????? ???????.</summary>
  public static (double AvgSec, decimal Volume, int Trades) ExpectedTradeMetrics(
      this IEnumerable<EscrowOrderEntity> orders,
      DateTime settledTs)
  {
    var deltas = orders.Select(o => (settledTs - o.CreatedAtUtc).TotalSeconds).ToArray();
    var volume = orders.Sum(o => o.FilledQuantity * (decimal)o.Price);
    return (deltas.Average(), volume, deltas.Length);
  }

}