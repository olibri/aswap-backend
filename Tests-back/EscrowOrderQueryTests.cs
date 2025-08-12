using Domain.Enums;
using Domain.Interfaces.Database.Queries;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.Offers;

namespace Tests_back;

public class EscrowOrderQueryTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  private async Task SeedAsync(int n, EscrowStatus st, OrderSide side, string fiat, CancellationToken ct = default)
  {
    var db = fixture.GetService<P2PDbContext>();
    await db.SeedAsync(n, st, "So111...", side, false, ct);
  }

  [Fact]
  public async Task Pagination_Works()
  {
    PostgresDatabase.ResetState("escrow_orders");
    await SeedAsync(30, EscrowStatus.OnChain, OrderSide.Sell, "USD");

    var queries = fixture.GetService<IMarketDbQueries>();
    var q = new OffersQuery().With(2, 10); // стор.2 по 10

    var page = await queries.GetAllNewOffersAsync(q);
    page.Length.ShouldBe(10);
  }

  [Fact]
  public async Task Sorting_ByPrice_Asc_Works()
  {
    PostgresDatabase.ResetState("escrow_orders");
    await SeedAsync(15, EscrowStatus.OnChain, OrderSide.Sell, "USD");

    var queries = fixture.GetService<IMarketDbQueries>();
    var q = new OffersQuery().With(sort: OfferSortField.Price, dir: SortDir.Asc);

    var res = await queries.GetAllNewOffersAsync(q);
    var prices = res.Select(o => o.Price).ToList();
    prices.ShouldBe(prices.OrderBy(p => p).ToList());
  }

  [Fact]
  public async Task Filter_ByStatus_And_Side_Works()
  {
    PostgresDatabase.ResetState("escrow_orders");

    await SeedAsync(10, EscrowStatus.OnChain, OrderSide.Sell, "USD");
    await SeedAsync(5, EscrowStatus.OnChain, OrderSide.Buy, "USD");

    var db = fixture.GetService<P2PDbContext>();
    var totalRows = await db.EscrowOrders.CountAsync();
    totalRows.ShouldBe(15);

    var queries = fixture.GetService<IMarketDbQueries>();
    var q = new OffersQuery().With(status: EscrowStatus.OnChain, side: OrderSide.Buy);


    var res = await queries.GetAllNewOffersAsync(q);

    res.Length.ShouldBe(5);
    res.All(o => o.Status == EscrowStatus.OnChain && o.OfferSide == OrderSide.Buy).ShouldBeTrue();
  }

  [Fact]
  public async Task Filter_By_PriceFrom_Works()
  {
    PostgresDatabase.ResetState("escrow_orders");
    await SeedAsync(25, EscrowStatus.OnChain, OrderSide.Sell, "USD");

    var db = fixture.GetService<P2PDbContext>();

    var prices = await db.EscrowOrders
      .OrderBy(o => o.Price)
      .Select(o => o.Price)
      .ToListAsync();

    var orders = await db.EscrowOrders
      .ToListAsync();

    prices.Count.ShouldBeGreaterThan(0);

    var thresholdNative = prices[prices.Count / 3];

    var thresholdForQuery = Convert.ToDecimal(thresholdNative);
    var queries = fixture.GetService<IMarketDbQueries>();
    var q = new OffersQuery().With(priceFrom: thresholdForQuery);

    var res = await queries.GetAllNewOffersAsync(q);

    res.Length.ShouldBeGreaterThan(0);

    res.All(o => o.Price * 100 >= thresholdNative).ShouldBeTrue();
  }

  [Fact]
  public async Task Filter_By_PaymentMethods_OR_Works()
  {
    PostgresDatabase.ResetState("escrow_orders");

    var db = fixture.GetService<P2PDbContext>();

    await SeedAsync(6, EscrowStatus.OnChain, OrderSide.Sell, "USD");
    var orders = await db.EscrowOrders
      .Include(o => o.PaymentMethods)
      .ThenInclude(pm => pm.Method)
      .ToListAsync();

    var codes = orders.SelectMany(o => o.PaymentMethods)
      .Select(pm => pm.Method!.Code)
      .Distinct()
      .Take(2)
      .ToArray();
    var filter = codes;
    var codeSet = codes.ToHashSet(StringComparer.OrdinalIgnoreCase);

    var expectedIds = orders
      .Where(o => o.PaymentMethods.Any(pm => pm.Method != null && codeSet.Contains(pm.Method.Code)))
      .Select(o => o.Id)
      .ToHashSet();

    var queries = fixture.GetService<IMarketDbQueries>();
    var q = new OffersQuery().With(paymentMethod: filter);

    var res = await queries.GetAllNewOffersAsync(q);

    res.Length.ShouldBe(expectedIds.Count);
    res.All(o => expectedIds.Contains(o.Id)).ShouldBeTrue();
  }
}