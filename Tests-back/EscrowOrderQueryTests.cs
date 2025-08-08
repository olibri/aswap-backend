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
    await db.SeedAsync(n, st, tokenMint: "So111...", side: side, partialFill: false, ct: ct);
  }

  [Fact]
  public async Task Pagination_Works()
  {
    PostgresDatabase.ResetState("escrow_orders");
    await SeedAsync(30, EscrowStatus.OnChain, OrderSide.Sell, "USD");

    var queries = fixture.GetService<IMarketDbQueries>();
    var q = new OffersQuery().With(page: 2, size: 10);   // стор.2 по 10

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
}