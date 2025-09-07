using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Tests_back.Extensions;
using Tests_back.Extensions.Offers;

namespace Tests_back;

public class EscrowOrderTests(TestFixture fixture) : IClassFixture<TestFixture>
{
  [Fact]
  public async Task QuickNodeCallback_ReturnsOk()
  {
    PostgresDatabase.ResetState("escrow_orders");

    //arrange
    var db = fixture.GetService<IMarketDbQueries>();
    var result = await OffersExtensions.CreateFakeOrder(fixture);

    //act
    var offer = await db.GetNewOfferAsync(1747314431853);

    //assert
    result.ShouldBeOfType<OkResult>();
    offer.ShouldNotBeNull();
    offer.DealId.ShouldBe(1747314431853UL);
    offer.FiatCode.ShouldBe("USD");
    offer.Amount.ShouldBe(1UL);
    offer.Price.ShouldBe(3m);
    offer.Status.ShouldBe(EscrowStatus.PendingOnChain);
    offer.BuyerFiat.ShouldBeNull();
    offer.OfferSide.ShouldBe(OrderSide.Sell);
  }

  [Fact]
  public async Task GetAllOffersTest()
  {
    PostgresDatabase.ResetState("escrow_orders");

    var ordersCount = 5;
    var controller = fixture.GetService<PlatformController>();
    await OffersExtensions.CreateFakeOrders(fixture, ordersCount);

    var result = await controller.GetAllNewOffers(new OffersQuery(), CancellationToken.None);

    result.ShouldNotBeNull();
    result.ShouldBeOfType<OkObjectResult>();

    var okResult = result as OkObjectResult;
    okResult.Value.ShouldBeOfType<EscrowOrderDto[]>();
    var offers = okResult.Value as EscrowOrderDto[];
    offers.Length.ShouldBe(ordersCount);
  }

  [Fact]
  public async Task PartialUpdateOffer()
  {
    PostgresDatabase.ResetState("escrow_orders");
    var controller = fixture.GetService<OrderController>();
    var marketDbQueries = fixture.GetService<IMarketDbQueries>();
    await OffersExtensions.CreateFakeOrder(fixture);

    var updateOrderDto = new UpsertOrderDto()
    {
      OrderId = 1747314431853UL,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      Status = EscrowStatus.OnChain,
      Buyer = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      PaymentMethodIds = [1,2]
    };

    var result = await controller.UpdateOffers(updateOrderDto);
    result.ShouldNotBeNull();
    result.ShouldBeOfType<OkResult>();

    var updatedOrder = await marketDbQueries.GetAllNewOffersAsync(new OffersQuery());
    Console.WriteLine($"Updated order: {updatedOrder[0].Amount}, {updatedOrder[0].FilledQuantity}");
    updatedOrder.ShouldNotBeNull();
    updatedOrder[0].MinFiatAmount.ShouldBe(10);
    updatedOrder[0].MaxFiatAmount.ShouldBe(10000);
    updatedOrder[0].Status.ShouldBe(EscrowStatus.OnChain);
    updatedOrder[0].BuyerFiat.ShouldBe("wallet0xzzzz");
    updatedOrder[0].FilledQuantity.ShouldBe(0.1m);
    updatedOrder[0].PaymentMethods.Count.ShouldBeGreaterThan(1);
  }

  [Fact]
  public async Task QuantityUpdateOffer()
  {
    PostgresDatabase.ResetState("escrow_orders");
    var marketDbCommand = fixture.GetService<IMarketDbCommand>();
    var marketDbQuery = fixture.GetService<IMarketDbQueries>();
    await OffersExtensions.CreateFakeOrder(fixture);

    var updateOrderDto1 = new UpsertOrderDto()
    {
      OrderId = 1747314431853UL,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      Status = EscrowStatus.OnChain,
      Buyer = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      PaymentMethodIds = [1,2]
    };

    await marketDbCommand.UpdateCurrentOfferAsync(updateOrderDto1);
    var updatedOrder1 = await marketDbQuery.GetAllNewOffersAsync(new OffersQuery());
    Console.WriteLine($"Updated order: {updatedOrder1[0].Amount}, {updatedOrder1[0].FilledQuantity}");
    updatedOrder1[0].FilledQuantity.ShouldBe(0.1m);
    updatedOrder1[0].PaymentMethods.Count.ShouldBeGreaterThan(1);


    var updateOrderDto2 = new UpsertOrderDto()
    {
      OrderId = 1747314431853UL,
      FilledQuantity = 0.9m
    };
    await marketDbCommand.UpdateCurrentOfferAsync(updateOrderDto2);
    var updatedOrder2 = await marketDbQuery.GetAllNewOffersAsync(new OffersQuery());
    updatedOrder2.ShouldBeEmpty();
  }

  [Fact]
  public async Task CreateBuyerOrderTest()
  {
    PostgresDatabase.ResetState("escrow_orders");
    var marketDbCommand = fixture.GetService<IMarketDbCommand>();
    var dealId = 1747314411853UL;

    var updateOrderDto = new UpsertOrderDto()
    {
      OrderId = dealId,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      FiatCode = "USD",
      Status = EscrowStatus.OnChain,
      Buyer = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      OrderSide = OrderSide.Buy,
      TokenMint = "tokenMintExample",
      Amount = 32m,
      PaymentMethodIds = [2,4]
    };

    var createdDealId = await marketDbCommand.CreateBuyerOfferAsync(updateOrderDto);
    createdDealId.ShouldBe(dealId);
    var marketDbQuery = fixture.GetService<IMarketDbQueries>();
    var createdOrder = await marketDbQuery.GetNewOfferAsync(dealId);
    createdOrder.ShouldNotBeNull();
    createdOrder.DealId.ShouldBe(dealId);
    createdOrder.PaymentMethods.Count.ShouldBeGreaterThan(1);
  }
}