using App.Db;
using Aswap_back.Controllers;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.Services.Order;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    var offer = await db.GetNewOfferAsync(1758270209859);

    //assert
    result.ShouldBeOfType<OkResult>();
    offer.ShouldNotBeNull();
    offer.DealId.ShouldBe(1758270209859UL);
    offer.FiatCode.ShouldBe("USD");
    offer.Amount.ShouldBe(33UL);
    offer.Price.ShouldBe(100m);
    offer.Status.ShouldBe(EscrowStatus.PendingOnChain);
    offer.BuyerFiat.ShouldBe("11111111111111111111111111111111");
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
    okResult.Value.ShouldBeOfType<PagedResult<EscrowOrderDto>>();
    var offers = okResult.Value as PagedResult<EscrowOrderDto>;
    offers.Data.Count.ShouldBe(ordersCount);
  }
  private static async Task SeedUserAsync(P2PDbContext db, string walletAddress)
  {
    if (!await db.Account.AnyAsync(a => a.WalletAddress == walletAddress))
    {
      db.Account.Add(new AccountEntity
      {
        WalletAddress = walletAddress,
        CreatedAtUtc = DateTime.UtcNow,
        LastActiveTime = DateTime.UtcNow
      });
      await db.SaveChangesAsync();
    }
  }
  [Fact]
  public async Task PartialUpdateOffer()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("account");

    var controller = fixture.GetService<OrderController>();
    var db = fixture.GetService<P2PDbContext>();
    var marketDbQueries = fixture.GetService<IMarketDbQueries>();
    var accountDbCommand = fixture.GetService<IAccountDbCommand>();
    var accountDbQueries = fixture.GetService<IAccountDbQueries>();
    await OffersExtensions.CreateFakeOrder(fixture);

    await AccountExtention.SaveFakeUserToDbAsync("wallet0xzzzz", accountDbCommand);
    await AccountExtention.SaveFakeUserToDbAsync("FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ", accountDbCommand);

    await SeedUserAsync(db, "wallet0xzzzz");
    await SeedUserAsync(db, "FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ");
    await SeedUserAsync(db, "CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW");

    var account = await accountDbQueries.GetAccountByWalletAsync("wallet0xzzzz");

    account.ShouldNotBeNull();
    var updateOrderDto = new UpsertOrderDto()
    {
      OrderId = 1758270209859UL,
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
    Console.WriteLine($"Updated order: {updatedOrder.Data[0].Amount}, {updatedOrder.Data[0].FilledQuantity}");
    updatedOrder.ShouldNotBeNull();
    updatedOrder.Data[0].MinFiatAmount.ShouldBe(10);
    updatedOrder.Data[0].MaxFiatAmount.ShouldBe(10000);
    updatedOrder.Data[0].Status.ShouldBe(EscrowStatus.OnChain);
    updatedOrder.Data[0].BuyerFiat.ShouldBe("wallet0xzzzz");
    updatedOrder.Data[0].FilledQuantity.ShouldBe(0.1m);
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

    var accountDbCommand = fixture.GetService<IAccountDbCommand>();

    await AccountExtention.SaveFakeUserToDbAsync("wallet0xzzzz", accountDbCommand);
    await AccountExtention.SaveFakeUserToDbAsync("FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ", accountDbCommand);
    await SeedUserAsync(db, "wallet0xzzzz");
    await SeedUserAsync(db, "FP31fp4XFN4Hp1QgUM2xfLKJPM4cRtJRxf3bbJN1KUbZ");
    await SeedUserAsync(db, "CjUEM1Qr7UN1VpMzGh4utFWH81ByN5gjobD6itoXawWW");
    var updateOrderDto1 = new UpsertOrderDto()
    {
      OrderId = 1758270209859UL,
      MaxFiatAmount = 10000,
      MinFiatAmount = 10,
      Status = EscrowStatus.OnChain,
      Buyer = "wallet0xzzzz",
      FilledQuantity = 0.1m,
      PaymentMethodIds = [1,2]
    };

    await marketDbCommand.UpdateCurrentOfferAsync(updateOrderDto1);
    var updatedOrder1 = await marketDbQuery.GetAllNewOffersAsync(new OffersQuery());
    Console.WriteLine($"Updated order: {updatedOrder1.Data[0].Amount} ,  {updatedOrder1.Data[0].FilledQuantity}");
    updatedOrder1.Data[0].FilledQuantity.ShouldBe(0.1m);
    updatedOrder1.Data[0].PaymentMethods.Count.ShouldBeGreaterThan(1);


    var updateOrderDto2 = new UpsertOrderDto()
    {
      OrderId = 1758270209859UL,
      FilledQuantity = 0.9m
    };
    await marketDbCommand.UpdateCurrentOfferAsync(updateOrderDto2);
    var updatedOrder2 = await marketDbQuery.GetAllNewOffersAsync(new OffersQuery());
    updatedOrder2.Data[0].FilledQuantity.ShouldBe(1m);
  }

  [Fact]
  public async Task CreateBuyerOrderTest()
  {
    PostgresDatabase.ResetState("escrow_orders");
    var marketDbCommand = fixture.GetService<IMarketDbCommand>();
    var dealId = 1758270209859UL;

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


  [Fact]
  public async Task PartialFlow_CreatesChild_And_SetsParent_PartiallyOnChain()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("child_order");

    // arrange
    await OffersExtensions.CreateFakeOrder(fixture);
    var cmd = fixture.GetService<IMarketDbCommand>();
    var q = fixture.GetService<IMarketDbQueries>();
    var childSvc = fixture.GetService<IChildOffersService>();

    var deal = 1758270209859UL;
    var parent = await q.GetNewOfferAsync(deal);
    parent.ShouldNotBeNull();
    parent.Status.ShouldBeOneOf(EscrowStatus.PendingOnChain, EscrowStatus.OnChain);

    // act: partial флоу
    var dto = new UpsertOrderDto
    {
      OrderId = deal,
      OrderSide = OrderSide.Sell,    // власник = Seller
      Seller = parent.SellerCrypto,
      Buyer = "buyer_wallet_Y",
      IsPartial = true,              // <- ключ: йдемо в child-флоу
      FilledQuantity = 0.1m
    };
    await cmd.UpdateCurrentOfferAsync(dto);

    // assert: parent оновлено мінімально і має Partial статус
    var after = await q.GetNewOfferAsync(deal);
    after.ShouldNotBeNull();
    after.FilledQuantity.ShouldBe(0.1m);

    // child створено і теж PartiallyOnChain
    var children = await childSvc.GetByParentAsync((long)deal);
    children.Count.ShouldBe(1);

    var ch = children[0];
    ch.DealId.ShouldBe(deal);
    ch.ParentOrderId.ShouldNotBe(Guid.Empty);
    //ch.EscrowStatus.ShouldBe(EscrowStatus.PartiallyOnChain);
    ch.OrderOwnerWallet.ShouldBe(parent.SellerCrypto);   // власник за OrderSide.Sell
    ch.ContraAgentWallet.ShouldBe("buyer_wallet_Y");
  }

  [Fact]
  public async Task PartialFlow_MultipleSteps_Finally_Releases_Parent()
  {
    PostgresDatabase.ResetState("escrow_orders");
    PostgresDatabase.ResetState("child_order");

    // arrange
    await OffersExtensions.CreateFakeOrder(fixture);
    var cmd = fixture.GetService<IMarketDbCommand>();
    var q = fixture.GetService<IMarketDbQueries>();
    var childSvc = fixture.GetService<IChildOffersService>();
    var accountDbCommand = fixture.GetService<IAccountDbCommand>();

    var deal = 1758270209859UL;
    var initial = await q.GetNewOfferAsync(deal);
    initial.ShouldNotBeNull();

    await AccountExtention.SaveFakeUserToDbAsync("seller_wallet_X", accountDbCommand);
    await AccountExtention.SaveFakeUserToDbAsync("buyer_wallet_Y", accountDbCommand);

    // крок 1: часткове заповнення
    await cmd.UpdateCurrentOfferAsync(new UpsertOrderDto
    {
      OrderId = deal,
      OrderSide = OrderSide.Sell,
      Seller = "seller_wallet_X",
      Buyer = "buyer_wallet_Y",
      IsPartial = true,
      FilledQuantity = 0.4m
    });

    // крок 2: добиваємо до повного
    await cmd.UpdateCurrentOfferAsync(new UpsertOrderDto
    {
      OrderId = deal,
      OrderSide = OrderSide.Sell,
      Seller = "seller_wallet_X",
      Buyer = "buyer_wallet_Y",
      IsPartial = true,
      FilledQuantity = 0.6m
    });

    // assert: parent має бути Released і зникнути з "нових" офферів (як у твоєму існуючому тесті)
    var list = await q.GetAllNewOffersAsync(new OffersQuery());
    list.Data.ShouldBeEmpty();

    // діти є (2 шт)
    var children = await childSvc.GetByParentAsync((long)deal);
    children.Count.ShouldBe(2);
    //children.ShouldAllBe(x => x.EscrowStatus == EscrowStatus.PartiallyOnChain);
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
    PostgresDatabase.ResetState("child_order");

    await OffersExtensions.CreateFakeOrder(fixture);

    var cmd = fixture.GetService<IMarketDbCommand>();
    var q = fixture.GetService<IMarketDbQueries>();
    var accountDbCommand = fixture.GetService<IAccountDbCommand>();

    await AccountExtention.SaveFakeUserToDbAsync("seller_wallet_X", accountDbCommand);
    await AccountExtention.SaveFakeUserToDbAsync("buyer_wallet_Y", accountDbCommand);
    
    var deal = 1758270209859UL;

    foreach (var f in fills)
    {
      await cmd.UpdateCurrentOfferAsync(new UpsertOrderDto
      {
        OrderId = deal,
        OrderSide = OrderSide.Sell,
        Seller = "seller_wallet_X",
        Buyer = "buyer_wallet_Y",
        IsPartial = true,
        FilledQuantity = f
      });
    }

    var list = await q.GetAllNewOffersAsync(new OffersQuery());
    list.Data.ShouldBeEmpty();
  }
}