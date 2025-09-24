using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Services.Account;
using Domain.Interfaces.Services.Order;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Infrastructure;
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
    //var offer1 = await db.GetAllNewOffersAsync(new OffersQuery());

    //act
    var offer = await db.GetNewOfferAsync(1758617998578);
    //"HgppLQ5K89ztCSj38zPFkkYKAvq3hEr4fFwps1QDSKu9"
    //assert
    result.ShouldBeOfType<OkResult>();
    offer.ShouldNotBeNull();
    offer.OrderId.ShouldBe(1758617998578UL);
    offer.FiatCode.ShouldBe("12");
    offer.Amount.ShouldBe(12UL);
    offer.Price.ShouldBe(0m);
    offer.Status.ShouldBe(UniversalOrderStatus.Created);
    offer.AcceptorWallet.ShouldBeNull();
    offer.OfferSide.ShouldBe(OrderSide.Buy);
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
    okResult.Value.ShouldBeOfType<PagedResult<EscrowOrderDto>>();
    var offers = okResult.Value as PagedResult<EscrowOrderDto>;
    offers.Data.Count.ShouldBe(ordersCount);
  }

  [Fact]
  public async Task PartialUpdateOffer()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("account");

    var controller = fixture.GetService<OrderController>();
    var db = fixture.GetService<P2PDbContext>();
    var marketDbQueries = fixture.GetService<IMarketDbQueries>();
    var _accountService = fixture.GetService<IAccountService>();
    var accountDbQueries = fixture.GetService<IAccountDbQueries>();
    await OffersExtensions.CreateFakeOrder(fixture);

    await AccountExtention.SaveFakeUserToDbAsync("wallet0xzzzz", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("HgppLQ5K89ztCSj38zPFkkYKAvq3hEr4fFwps1QDSKu9", _accountService);


    var account = await accountDbQueries.GetAccountByWalletAsync("wallet0xzzzz");

    account.ShouldNotBeNull();
    var updateOrderDto = new UpsertOrderDto()
    {
      OrderId = 1758617998578UL,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      Status = UniversalOrderStatus.Active,
      AcceptorWallet = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      PaymentMethodIds = [1, 2]
    };

    var result = await controller.UpdateOffers(updateOrderDto);
    result.ShouldNotBeNull();
    result.ShouldBeOfType<OkResult>();

    var updatedOrder = await marketDbQueries.GetAllNewOffersAsync(new OffersQuery());
    Console.WriteLine($"Updated order: {updatedOrder.Data[0].Amount}, {updatedOrder.Data[0].FilledQuantity}");
    updatedOrder.ShouldNotBeNull();
    updatedOrder.Data[0].MinFiatAmount.ShouldBe(10);
    updatedOrder.Data[0].MaxFiatAmount.ShouldBe(10000);
    updatedOrder.Data[0].Status.ShouldBe(UniversalOrderStatus.Active);
    updatedOrder.Data[0].AcceptorWallet.ShouldBe("wallet0xzzzz");
    updatedOrder.Data[0].FilledQuantity.ShouldBe(100000m);
    updatedOrder.Data[0].PaymentMethods.Count.ShouldBeGreaterThan(1);
  }

  [Fact]
  public async Task QuantityUpdateOffer()
  {
    PostgresDatabase.ResetState("escrow_orders");
    var marketDbCommand = fixture.GetService<IMarketDbCommand>();
    var marketDbQuery = fixture.GetService<IMarketDbQueries>();
    await OffersExtensions.CreateFakeOrder(fixture);
    var db = fixture.GetService<P2PDbContext>();

    var _accountService = fixture.GetService<IAccountService>();

    await AccountExtention.SaveFakeUserToDbAsync("wallet0xzzzz", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("HgppLQ5K89ztCSj38zPFkkYKAvq3hEr4fFwps1QDSKu9", _accountService);

    var updateOrderDto1 = new UpsertOrderDto()
    {
      OrderId = 1758617998578UL,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      Status = UniversalOrderStatus.Active,
      AcceptorWallet = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      PaymentMethodIds = [1, 2]
    };

    await marketDbCommand.UpdateCurrentOfferAsync(updateOrderDto1);
    var updatedOrder1 = await marketDbQuery.GetAllNewOffersAsync(new OffersQuery());
    Console.WriteLine($"Updated order: {updatedOrder1.Data[0].Amount} ,  {updatedOrder1.Data[0].FilledQuantity}");
    updatedOrder1.Data[0].FilledQuantity.ShouldBe(100000m);
    updatedOrder1.Data[0].PaymentMethods.Count.ShouldBeGreaterThan(1);


    var updateOrderDto2 = new UpsertOrderDto()
    {
      OrderId = 1758617998578UL,
      FilledQuantity = 0.9m
    };
    await marketDbCommand.UpdateCurrentOfferAsync(updateOrderDto2);
    var updatedOrder2 = await marketDbQuery.GetAllNewOffersAsync(new OffersQuery());
    updatedOrder2.Data[0].FilledQuantity.ShouldBe(1000000m);
  }

  [Fact]
  public async Task CreateBuyerOrderTest()
  {
    PostgresDatabase.ResetState("escrow_orders");
    var marketDbCommand = fixture.GetService<IMarketDbCommand>();
    var orderId = 1758617998578UL;

    var updateOrderDto = new UpsertOrderDto()
    {
      OrderId = orderId,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      FiatCode = "USD",
      Status = UniversalOrderStatus.Active,
      AcceptorWallet = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      OrderSide = OrderSide.Buy,
      TokenMint = "tokenMintExample",
      Amount = 32m,
      PaymentMethodIds = [2, 4]
    };

    var createdOrderId = await marketDbCommand.CreateBuyerOfferAsync(updateOrderDto);
    createdOrderId.ShouldBe(orderId);
    var marketDbQuery = fixture.GetService<IMarketDbQueries>();
    var createdOrder = await marketDbQuery.GetNewOfferAsync(orderId);
    createdOrder.ShouldNotBeNull();
    createdOrder.OrderId.ShouldBe(orderId);
    createdOrder.PaymentMethods.Count.ShouldBeGreaterThan(1);
  }


  [Fact]
  public async Task PartialFlow_CreatesChild_And_SetsParent_PartiallyOnChain()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("universal_tickets");

    // arrange
    await OffersExtensions.CreateFakeOrder(fixture);
    var cmd = fixture.GetService<IMarketDbCommand>();
    var q = fixture.GetService<IMarketDbQueries>();
    var childSvc = fixture.GetService<IChildOffersService>();
    
    var _accountService = fixture.GetService<IAccountService>();
    await AccountExtention.SaveFakeUserToDbAsync("HgppLQ5K89ztCSj38zPFkkYKAvq3hEr4fFwps1QDSKu9", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("buyer_wallet_Y", _accountService);

    var orderId = 1758617998578UL;
    var parent = await q.GetNewOfferAsync(orderId);
    parent.ShouldNotBeNull();
    parent.Status.ShouldBeOneOf(UniversalOrderStatus.Created, UniversalOrderStatus.Active);

    var dto = new UpsertOrderDto
    {
      OrderId = orderId,
      OrderSide = OrderSide.Sell, 
      CreatorWallet = parent.CreatorWallet,
      AcceptorWallet = "buyer_wallet_Y",
      IsPartial = true,
      FilledQuantity = 0.1m
    };
    await cmd.UpdateCurrentOfferAsync(dto);

    var after = await q.GetNewOfferAsync(orderId);
    after.ShouldNotBeNull();
    after.FilledQuantity.ShouldBe(100000m);

    var children = await childSvc.GetByParentAsync((long)orderId);
    children.Count.ShouldBe(1);

    var ch = children[0];
    ch.TicketId.ShouldBe(orderId);
    ch.ParentOrderId.ShouldNotBe(Guid.Empty);
    ch.OrderOwnerWallet.ShouldBe(parent.CreatorWallet); 
    ch.ContraAgentWallet.ShouldBe("buyer_wallet_Y");
  }

  [Fact]
  public async Task PartialFlow_MultipleSteps_Finally_Releases_Parent()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("universal_tickets");

    // arrange
    await OffersExtensions.CreateFakeOrder(fixture);
    var cmd = fixture.GetService<IMarketDbCommand>();
    var q = fixture.GetService<IMarketDbQueries>();
    var childSvc = fixture.GetService<IChildOffersService>();
    var _accountService = fixture.GetService<IAccountService>();

    var orderId = 1758617998578UL;
    var initial = await q.GetNewOfferAsync(orderId);
    initial.ShouldNotBeNull();

    await AccountExtention.SaveFakeUserToDbAsync("seller_wallet_X", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("buyer_wallet_Y", _accountService);

    // крок 1: часткове заповнення
    await cmd.UpdateCurrentOfferAsync(new UpsertOrderDto
    {
      OrderId = orderId,
      OrderSide = OrderSide.Sell,
      CreatorWallet = "seller_wallet_X",
      AcceptorWallet = "buyer_wallet_Y",
      IsPartial = true,
      FilledQuantity = 30m
    });

    // крок 2: добиваємо до повного
    await cmd.UpdateCurrentOfferAsync(new UpsertOrderDto
    {
      OrderId = orderId,
      OrderSide = OrderSide.Sell,
      Status = UniversalOrderStatus.Completed,
      CreatorWallet = "seller_wallet_X",
      AcceptorWallet = "buyer_wallet_Y",
      IsPartial = true,
      FilledQuantity = 3m
    });

    // assert: parent має бути Completed і зникнути з "нових" офферів
    var list = await q.GetAllNewOffersAsync(new OffersQuery());
    list.Data.ShouldBeEmpty();

  }


  public static IEnumerable<object[]> PartialSequences()
  {
    yield return [new[] { 1.0m }];
    yield return [new[] { 0.5m, 0.5m }];
    yield return [new[] { 0.2m, 0.3m, 0.5m }];
    yield return [new[] { 0.1m, 0.1m, 0.1m, 0.7m }];
  }

  [Theory]
  [MemberData(nameof(PartialSequences))]
  public async Task PartialFlow_Sequences_Reach_Release(decimal[] fills)
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("universal_tickets");

    await OffersExtensions.CreateFakeOrder(fixture);

    var cmd = fixture.GetService<IMarketDbCommand>();
    var q = fixture.GetService<IMarketDbQueries>();
    var _accountService = fixture.GetService<IAccountService>();

    await AccountExtention.SaveFakeUserToDbAsync("seller_wallet_X", _accountService);
    await AccountExtention.SaveFakeUserToDbAsync("buyer_wallet_Y", _accountService);

    var orderId = 1758617998578UL;

    foreach (var f in fills)
      await cmd.UpdateCurrentOfferAsync(new UpsertOrderDto
      {
        OrderId = orderId,
        OrderSide = OrderSide.Sell,
        CreatorWallet = "seller_wallet_X",
        AcceptorWallet = "buyer_wallet_Y",
        IsPartial = true,
        FilledQuantity = f
      });

    var list = await q.GetAllNewOffersAsync(new OffersQuery());
    list.Data.ShouldNotBeEmpty();
  }
}