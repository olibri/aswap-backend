using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Models.Api.Order;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.AccountAuth;
using Tests_back.Extensions.Order;

namespace Tests_back;

public class BestPriceControllerTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  private const string Sol = "So11111111111111111111111111111111111111112";
  private const string Usd = "USD";

  [Fact]
  public async Task BestPrice_Sell_Takes_LowestPrice()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, 1000);
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, 950);
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, 1100);

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Sell,
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldNotBeNull();
    dto!.Price.ShouldBe((ulong)9.50);
    dto.Side.ShouldBe(OrderSide.Sell);
  }

  [Fact]
  public async Task BestPrice_Buy_Takes_HighestPrice()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, 910);
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, 1075);
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, 1030);

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Buy,
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldNotBeNull();
    dto!.Price.ShouldBe((ulong)10.75);
    dto.Side.ShouldBe(OrderSide.Buy);
  }


  [Fact]
  public async Task BestPrice_Ignores_NonMatching_And_NonOnChain()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    var (m1, _) = await fixture.AnyTwoMethodsAsync();

    await fixture.AddOrderAsync("OtherMint", Usd, OrderSide.Sell, 100);
    await fixture.AddOrderAsync(Sol, "EUR", OrderSide.Sell, 100);
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, 100);
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, 100, UniversalOrderStatus.Cancelled,
      new[] { m1.Id });

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, 777);

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Sell,
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldNotBeNull();
    dto!.Price.ShouldBe((ulong)7.77);
  }

  [Fact]
  public async Task BestPrice_Returns_Null_When_No_Matches()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Sell,
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldBeNull();
  }
}