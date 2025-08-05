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

    var (m1, _) = await fixture.AnyTwoMethodsAsync();

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)10.00, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)9.50, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)11.00, methodIds: new[] { m1.Id });

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Sell,
      MethodIds = []
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldNotBeNull();
    dto!.Price.ShouldBe((ulong)9.50);
    dto.Side.ShouldBe(OrderSide.Sell);
    dto.MethodIds.ShouldContain(m1.Id);
  }

  [Fact]
  public async Task BestPrice_Buy_Takes_HighestPrice()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    var (m1, _) = await fixture.AnyTwoMethodsAsync();

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, (ulong)9.10, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, (ulong)10.75, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, (ulong)10.30, methodIds: new[] { m1.Id });

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Buy,
      MethodIds = []
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldNotBeNull();
    dto!.Price.ShouldBe((ulong)10.75);
    dto.Side.ShouldBe(OrderSide.Buy);
  }

  [Fact]
  public async Task BestPrice_Filters_By_PaymentMethod_When_Passed()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    var (m1, m2) = await fixture.AnyTwoMethodsAsync();

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)10.00, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)9.00, methodIds: new[] { m2.Id });

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Sell,
      MethodIds = new List<short> { m1.Id }
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldNotBeNull();
    dto!.Price.ShouldBe((ulong)10.00);
    dto.MethodIds.ShouldContain(m1.Id);
    dto.MethodIds.ShouldNotContain(m2.Id);
  }

  [Fact]
  public async Task BestPrice_Ignores_NonMatching_And_NonOnChain()
  {
    fixture.ResetDb("escrow_orders", "escrow_order_payment_methods");

    var (m1, _) = await fixture.AnyTwoMethodsAsync();

    await fixture.AddOrderAsync("OtherMint", Usd, OrderSide.Sell, (ulong)1.00, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, "EUR", OrderSide.Sell, (ulong)1.00, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Buy, (ulong)1.00, methodIds: new[] { m1.Id });
    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)1.00, EscrowStatus.Cancelled,
      new[] { m1.Id });

    await fixture.AddOrderAsync(Sol, Usd, OrderSide.Sell, (ulong)7.77, methodIds: new[] { m1.Id });

    var ctrl = fixture.GetService<OrderController>().WithUser("u1");

    var req = new BestPriceRequest
    {
      TokenMint = Sol,
      FiatCode = Usd,
      Side = OrderSide.Sell,
      MethodIds = []
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
      MethodIds = []
    };

    var dto = await ctrl.GetBestPrice(req).OkValueAsync<BestPriceDto>();
    dto.ShouldBeNull();
  }
}